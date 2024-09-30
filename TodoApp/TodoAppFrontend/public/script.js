document.addEventListener('DOMContentLoaded', function () {
    function fetchTodos() {
        fetch('/todos')
            .then(response => response.json())
            .then(data => {
                const todoList = document.getElementById('todo-list');
                todoList.innerHTML = '';
                data.forEach(todo => {
                    const li = document.createElement('li');

                    li.innerHTML = `
                        <span class="${todo.done ? 'done' : ''}">${todo.todoText}</span>
                        <span>
                            ${!todo.done ? `<button class="mark-done" data-id="${todo.id}">Mark as Done</button>` : ''}
                            <button class="delete-todo" data-id="${todo.id}">Delete</button>
                        </span>
                    `;

                    todoList.appendChild(li);
                });

                const markDoneButtons = document.querySelectorAll('.mark-done');
                markDoneButtons.forEach(button => {
                    button.addEventListener('click', function () {
                        const todoId = this.getAttribute('data-id');
                        markTodoAsDone(todoId);
                    });
                });

                const deleteButtons = document.querySelectorAll('.delete-todo');
                deleteButtons.forEach(button => {
                    button.addEventListener('click', function () {
                        const todoId = this.getAttribute('data-id');
                        deleteTodo(todoId);
                    });
                });
            })
            .catch(error => console.error('Error fetching todos:', error));
    }

    function markTodoAsDone(id) {
        fetch(`/todos/${id}`, {
            method: 'PUT'
        })
        .then(response => {
            if (response.ok) {
                fetchTodos();
            }
        })
        .catch(error => console.error('Error marking todo as done:', error));
    }

    function deleteTodo(id) {
        fetch(`/todos/${id}`, {
            method: 'DELETE'
        })
        .then(response => {
            if (response.ok) {
                fetchTodos();
            }
        })
        .catch(error => console.error('Error deleting todo:', error));
    }

    fetchTodos();
});
