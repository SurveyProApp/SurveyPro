document.addEventListener("DOMContentLoaded", () => {
    const questionForm = document.getElementById("questionForm");
    const optionsContainer = document.getElementById("optionsContainer");
    const optionsBlock = document.getElementById("optionsBlock");
    const questionTypeSelect = document.getElementById("questionType");

    if (!questionForm || !optionsContainer || !optionsBlock || !questionTypeSelect) {
        return;
    }

    function toggleForm() {
        questionForm.classList.toggle("survey-edit-hidden");
    }

    function toggleEditForm(id) {
        const form = document.getElementById("editForm-" + id);

        if (!form) {
            return;
        }

        form.classList.toggle("survey-edit-hidden");
    }

    function reindexOptions() {
        const inputs = optionsContainer.querySelectorAll("input");

        inputs.forEach((input, index) => {
            input.name = `Options[${index}]`;
        });
    }

    function reindexEditOptions(id) {
        const container = document.getElementById("editOptionsContainer-" + id);

        if (!container) {
            return;
        }

        container.querySelectorAll("input").forEach((input, index) => {
            input.name = `Options[${index}]`;
        });
    }

    function removeEditOption(button, id) {
        const container = document.getElementById("editOptionsContainer-" + id);

        if (!container || container.children.length <= 2) {
            return;
        }

        button.closest(".d-flex")?.remove();
        reindexEditOptions(id);
    }

    function addOption(value = "") {
        const index = optionsContainer.children.length;

        const wrapper = document.createElement("div");
        wrapper.className = "d-flex mt-1";

        const input = document.createElement("input");
        input.name = `Options[${index}]`;
        input.className = "form-control";
        input.placeholder = "Option";
        input.value = value;

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-danger ms-2";
        removeButton.innerText = "X";

        removeButton.addEventListener("click", () => {
            if (optionsContainer.children.length <= 2) {
                return;
            }

            wrapper.remove();
            reindexOptions();
        });

        wrapper.appendChild(input);
        wrapper.appendChild(removeButton);
        optionsContainer.appendChild(wrapper);
    }

    function addEditOption(id, value = "") {
        const container = document.getElementById("editOptionsContainer-" + id);

        if (!container) {
            return;
        }

        const index = container.children.length;

        const wrapper = document.createElement("div");
        wrapper.className = "d-flex mt-1 edit-option-row-" + id;

        const input = document.createElement("input");
        input.name = `Options[${index}]`;
        input.className = "form-control";
        input.placeholder = "Option";
        input.value = value;

        const removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-danger ms-2 js-remove-edit-option";
        removeButton.dataset.questionId = id;
        removeButton.innerText = "X";

        wrapper.appendChild(input);
        wrapper.appendChild(removeButton);
        container.appendChild(wrapper);
    }

    function handleTypeChange() {
        const type = questionTypeSelect.value;

        optionsContainer.innerHTML = "";

        if (type === "Text") {
            optionsBlock.classList.add("survey-edit-hidden");
            return;
        }

        if (type === "SingleChoice" || type === "MultipleChoice") {
            addOption();
            addOption();
        }

        optionsBlock.classList.remove("survey-edit-hidden");

        if (type === "Likert") {
            ["Strongly disagree", "Disagree", "Neutral", "Agree", "Strongly agree"]
                .forEach((text) => addOption(text));
        }
    }

    function handleEditTypeChange(id) {
        const typeSelect = document.getElementById("editType-" + id);
        const container = document.getElementById("editOptionsContainer-" + id);
        const block = document.getElementById("editOptionsBlock-" + id);

        if (!typeSelect || !container || !block) {
            return;
        }

        container.innerHTML = "";

        if (typeSelect.value === "Text") {
            block.classList.add("survey-edit-hidden");
            return;
        }

        block.classList.remove("survey-edit-hidden");

        if (typeSelect.value === "SingleChoice" || typeSelect.value === "MultipleChoice") {
            addEditOption(id);
            addEditOption(id);
        }

        if (typeSelect.value === "Likert") {
            ["Strongly disagree", "Disagree", "Neutral", "Agree", "Strongly agree"]
                .forEach((text) => addEditOption(id, text));
        }
    }

    function showError(element, message) {
        const span = document.createElement("span");
        span.className = "text-danger validation-error";
        span.innerText = message;
        element.parentNode.insertBefore(span, element.nextSibling);
    }

    function clearErrors(form) {
        form.querySelectorAll(".validation-error").forEach((el) => el.remove());
    }

    function validateEditForm(form, id) {
        const textInput = form.querySelector("input[name='Text']");
        const typeSelect = document.getElementById("editType-" + id);
        const container = document.getElementById("editOptionsContainer-" + id);

        if (!textInput || !typeSelect || !container) {
            return true;
        }

        clearErrors(form);

        let valid = true;

        if (!textInput.value.trim()) {
            showError(textInput, "Question text is required");
            valid = false;
        }

        if (typeSelect.value === "SingleChoice" || typeSelect.value === "MultipleChoice") {
            const inputs = container.querySelectorAll("input");
            const filled = Array.from(inputs).filter((input) => input.value.trim() !== "");

            if (filled.length < 2) {
                showError(container, "At least 2 options are required");
                valid = false;
            }
        }

        return valid;
    }

    function validateCreateForm(form) {
        const textInput = form.querySelector("input[name='Text']");
        const typeSelect = form.querySelector("select[name='Type']");

        if (!textInput || !typeSelect) {
            return true;
        }

        clearErrors(form);

        let valid = true;

        if (!textInput.value.trim()) {
            showError(textInput, "Question text is required");
            valid = false;
        }

        if (typeSelect.value === "SingleChoice" || typeSelect.value === "MultipleChoice") {
            const inputs = optionsContainer.querySelectorAll("input");
            const filled = Array.from(inputs).filter((input) => input.value.trim() !== "");

            if (filled.length < 2) {
                showError(optionsContainer, "At least 2 options are required");
                valid = false;
            }
        }

        return valid;
    }

    document.addEventListener("click", (event) => {
        const toggleQuestionButton = event.target.closest(".js-toggle-question-form");

        if (toggleQuestionButton) {
            toggleForm();
            return;
        }

        const toggleEditButton = event.target.closest(".js-toggle-edit-form");

        if (toggleEditButton) {
            const id = toggleEditButton.dataset.questionId;

            if (id) {
                toggleEditForm(id);
            }

            return;
        }

        const addOptionButton = event.target.closest(".js-add-option");

        if (addOptionButton) {
            addOption();
            return;
        }

        const addEditOptionButton = event.target.closest(".js-add-edit-option");

        if (addEditOptionButton) {
            const id = addEditOptionButton.dataset.questionId;

            if (id) {
                addEditOption(id);
            }

            return;
        }

        const removeEditOptionButton = event.target.closest(".js-remove-edit-option");

        if (removeEditOptionButton) {
            const id = removeEditOptionButton.dataset.questionId;

            if (id) {
                removeEditOption(removeEditOptionButton, id);
            }
        }
    });

    document.addEventListener("change", (event) => {
        if (event.target.closest(".js-question-type")) {
            handleTypeChange();
            return;
        }

        const editTypeSelect = event.target.closest(".js-edit-type");

        if (editTypeSelect) {
            const id = editTypeSelect.dataset.questionId;

            if (id) {
                handleEditTypeChange(id);
            }
        }
    });

    document.querySelectorAll(".js-edit-question-form").forEach((form) => {
        form.addEventListener("submit", (event) => {
            const id = form.dataset.questionId;

            if (id && !validateEditForm(form, id)) {
                event.preventDefault();
            }
        });
    });

    const createForm = document.querySelector(".js-create-question-form");

    createForm?.addEventListener("submit", (event) => {
        if (!validateCreateForm(createForm)) {
            event.preventDefault();
        }
    });

    handleTypeChange();
});
