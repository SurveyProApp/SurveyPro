document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".js-delete-response-form").forEach((form) => {
    form.addEventListener("submit", (event) => {
      const confirmed = window.confirm("Delete this response?");

      if (!confirmed) {
        event.preventDefault();
      }
    });
  });
});
