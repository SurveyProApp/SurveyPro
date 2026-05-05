document.addEventListener('DOMContentLoaded', () => {
    const deleteModal = document.getElementById('deleteModal');

    deleteModal.addEventListener('show.bs.modal', function (event) {

        const button = event.relatedTarget;

        const id = button.getAttribute('data-id');
        const title = button.getAttribute('data-title');

        document.getElementById('surveyId').value = id;
        document.getElementById('surveyTitle').textContent = title;
    });
});