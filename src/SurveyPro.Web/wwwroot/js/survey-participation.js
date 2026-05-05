document.addEventListener("DOMContentLoaded", () => {
  const submitForm = document.querySelector('form[action*="Submit"]');

  function clearText(i) {
    const el = document.querySelector(`[name="Questions[${i}].TextAnswer"]`);

    if (!el) return;

    el.value = "";
    el.dispatchEvent(new Event("input"));
  }

  function clearRadio(i) {
    document
      .querySelectorAll(`[name="Questions[${i}].SelectedOptionId"]`)
      .forEach((x) => {
        x.checked = false;
        x.dispatchEvent(new Event("change"));
      });
  }

  function clearCheckbox(i) {
    document
      .querySelectorAll(`[name="Questions[${i}].SelectedOptionIds"]`)
      .forEach((x) => {
        x.checked = false;
        x.dispatchEvent(new Event("change"));
      });
  }

  document.addEventListener("click", (event) => {
    const clearButton = event.target.closest(".js-clear-answer");

    if (!clearButton) {
      return;
    }

    const index = clearButton.dataset.questionIndex;
    const clearType = clearButton.dataset.clearType;

    if (index === undefined) {
      return;
    }

    if (clearType === "text") {
      clearText(index);
      return;
    }

    if (clearType === "checkbox") {
      clearCheckbox(index);
      return;
    }

    if (clearType === "radio") {
      clearRadio(index);
    }
  });

  let timeout = null;

  function collectFormData() {
    const questions = [];

    document
      .querySelectorAll(".survey-question-card")
      .forEach((card, index) => {
        const questionIdEl = card.querySelector(
          `[name="Questions[${index}].QuestionId"]`,
        );

        const typeEl = card.querySelector(`[name="Questions[${index}].Type"]`);

        if (!questionIdEl || !typeEl) {
          return;
        }

        const questionId = questionIdEl.value;
        const type = typeEl.value;

        let textAnswer = null;
        let selectedOptionId = null;
        const selectedOptionIds = [];

        const textarea = card.querySelector("textarea");

        if (textarea) {
          textAnswer = textarea.value;
        }

        const radio = card.querySelector('input[type="radio"]:checked');

        if (radio) {
          selectedOptionId = radio.value;
        }

        card
          .querySelectorAll('input[type="checkbox"]:checked')
          .forEach((cb) => selectedOptionIds.push(cb.value));

        questions.push({
          questionId,
          type,
          textAnswer,
          selectedOptionId,
          selectedOptionIds,
        });
      });

    return {
      surveyId: document.querySelector('[name="SurveyId"]')?.value,

      accessCode: document.querySelector('[name="AccessCode"]')?.value,

      questions,
    };
  }

  async function autoSave() {
    clearTimeout(timeout);

    timeout = setTimeout(async () => {
      console.log("sending request");

      const data = collectFormData();

      try {
        const response = await fetch("/Participation/SaveDraft", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            RequestVerificationToken: document.querySelector(
              'input[name="__RequestVerificationToken"]',
            ).value,
          },
          body: JSON.stringify(data),
        });

        if (response.ok) {
          showSavedIndicator();
        }
      } catch (err) {
        console.error(err);
      }
    }, 800);
  }

  function showSavedIndicator() {
    let el = document.getElementById("autosave-status");

    if (!el) {
      el = document.createElement("div");
      el.id = "autosave-status";
      el.className = "autosave-status";
      el.innerText = "Saved ✓";

      document.body.appendChild(el);
    }

    el.style.opacity = "1";

    setTimeout(() => {
      el.style.opacity = "0";
    }, 1500);
  }

  document.querySelectorAll('textarea, input[type="text"]').forEach((el) => {
    el.addEventListener("input", autoSave);
  });

  document
    .querySelectorAll('select, input[type="checkbox"], input[type="radio"]')
    .forEach((el) => {
      el.addEventListener("change", autoSave);
    });

  submitForm?.addEventListener("submit", function (e) {
    clearTimeout(timeout);

    const errors = [];

    document
      .querySelectorAll(".survey-question-card")
      .forEach((card, index) => {
        const typeEl = card.querySelector(`[name="Questions[${index}].Type"]`);

        if (!typeEl) {
          return;
        }

        const type = typeEl.value;

        const orderEl = card.querySelector("h5");

        const label = orderEl
          ? orderEl.innerText.trim()
          : `Question ${index + 1}`;

        if (type === "Text") {
          const textarea = card.querySelector("textarea");

          if (!textarea || !textarea.value.trim()) {
            errors.push(label);
          }
        } else if (type === "MultipleChoice") {
          const checked = card.querySelectorAll(
            'input[type="checkbox"]:checked',
          );

          if (checked.length === 0) {
            errors.push(label);
          }
        } else {
          const checked = card.querySelector('input[type="radio"]:checked');

          if (!checked) {
            errors.push(label);
          }
        }
      });

    if (errors.length > 0) {
      e.preventDefault();

      let alertEl = document.getElementById("submit-validation-error");

      if (!alertEl) {
        alertEl = document.createElement("div");

        alertEl.id = "submit-validation-error";

        alertEl.className = "alert alert-danger mt-2";

        submitForm.before(alertEl);
      }

      alertEl.innerHTML = `
                <strong>
                    Please answer all questions before submitting.
                </strong>

                <ul class="mb-0 mt-1">
                    ${errors.map((e) => `<li>${e}</li>`).join("")}
                </ul>
            `;

      alertEl.scrollIntoView({
        behavior: "smooth",
        block: "center",
      });
    }
  });
});
