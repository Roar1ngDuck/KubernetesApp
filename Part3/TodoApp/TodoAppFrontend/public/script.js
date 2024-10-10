document.addEventListener('DOMContentLoaded', function () {
    function fetchTodos() {
        fetch('/todos')
            .then(response => response.json())
            .then(data => {
                const todoList = document.getElementById('todo-list');
                todoList.innerHTML = '';
                data.forEach(todo => {
                    const li = document.createElement('li');
                    li.textContent = todo;
                    todoList.appendChild(li);
                });
            })
            .catch(error => console.error('Error fetching todos:', error));
    }

    fetchTodos();
});