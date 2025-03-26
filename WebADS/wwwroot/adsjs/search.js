document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('searchButton').addEventListener('click', handleSearch);
    restoreSearch();
    const searchInput = document.getElementById('search');
    const clearButton = document.getElementById('clearSearch');

    // Функция для обновления видимости кнопки очистки
    function updateClearButtonVisibility() {
        if (searchInput.value.trim() !== '') {
            clearButton.classList.remove('hidden');
        } else {
            clearButton.classList.add('hidden');
        }
    }

    // Очистка поля ввода и результатов поиска
    clearButton.addEventListener('click', function () {
        searchInput.value = ''; // Очищаем поле ввода
        clearButton.classList.add('hidden'); // Скрываем кнопку очистки

        // Очищаем LocalStorage (опционально)
        localStorage.removeItem('searchText');
    });

    // Обновляем видимость кнопки при вводе текста
    searchInput.addEventListener('input', updateClearButtonVisibility);

    // Обработка нажатия Enter в поле ввода
    searchInput.addEventListener('keypress', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault(); // Предотвращаем стандартное поведение формы
            document.getElementById('searchButton').click(); // Имитируем клик по кнопке поиска
        }
    });

    // Инициализация видимости кнопки при загрузке страницы
    updateClearButtonVisibility();
});


function handleSearch(e) {
    if (e && e.preventDefault) {
        e.preventDefault(); // Отменяем стандартную отправку формы, если событие передано
    }

    const searchInput = document.getElementById('search'); // Находим поле поиска
    const searchQuery = searchInput.value.trim(); // Получаем текст поиска и удаляем лишние пробелы

    // Проверяем, что запрос не пустой
    if (!searchQuery) {
        alert('Введите запрос для поиска.');
        return;
    }

    // Показываем индикатор загрузки
    const loadingIndicator = document.getElementById('loadingIndicator');
    loadingIndicator.style.display = 'block';

    // Выполняем поиск через AJAX
    fetch('/ADSIndex?handler=Search&search=' + encodeURIComponent(searchQuery), {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            // Скрываем индикатор загрузки
            loadingIndicator.style.display = 'none';

            if (data && data.length > 0) {
                showSearchResults(data); // Отображаем результаты в блоке
            } else {
                alert('Ничего не найдено.');
            }
        })
        .catch(error => {
            // Скрываем индикатор загрузки в случае ошибки
            loadingIndicator.style.display = 'none';
            console.error('Ошибка при выполнении поиска:', error);
        });
}

// Функция для отображения результатов поиска в блоке
function showSearchResults(results) {
    const tbody = document.getElementById('searchResultsBody');
    tbody.innerHTML = ''; // Очищаем предыдущие результаты

    // Заполняем таблицу данными
    results.forEach(product => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${product.name}</td>
            <td>${product.article}</td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="redirectToAdsSLM('${product.id}')">Выбрать</button>
            </td>
        `;
        tbody.appendChild(row);
    });

    // Показываем блок с результатами (если он был скрыт)
    const searchResultsBlock = document.querySelector('.search-results-block');
    if (searchResultsBlock) {
        searchResultsBlock.style.display = 'block'; // Делаем блок видимым
    }
}

// Функция для перенаправления на страницу AdsSLMModel с параметром moySkladId
function redirectToAdsSLM(moySkladId) {

    const searchText = document.getElementById('search').value;
    localStorage.setItem('searchText', searchText);
    
    window.location.href = `/AdsSLM?moySkladId=${moySkladId}`;
}

// Функция для очистки результатов поиска
function clearSearchResults() {
    const tbody = document.getElementById('searchResultsBody');
    tbody.innerHTML = ''; // Очищаем таблицу

    // Скрываем блок с результатами (опционально)
    const searchResultsBlock = document.querySelector('.search-results-block');
    if (searchResultsBlock) {
        searchResultsBlock.style.display = 'none';
    }
}

// Функция для восстановления текста поиска и выполнения поиска
function restoreSearch() {
    const searchText = localStorage.getItem('searchText');
    if (searchText) {
        // Устанавливаем текст в поле поиска
        document.getElementById('search').value = searchText;

        // Выполняем поиск
        handleSearch();

        // Очищаем LocalStorage (опционально)
        //localStorage.removeItem('searchText');
    }
}

//document.getElementById('selectStoreForm').addEventListener('submit', function (e) {
//    e.preventDefault(); // Отменяем стандартную отправку формы

//    submitStoreForm(); 

//});

//function submitStoreForm() {
//    const selectedProduct = JSON.parse(sessionStorage.getItem('selectedProduct'));
//    if (!selectedProduct) {
//        alert('Выберите продукт перед изменением склада.');
//        return;
//    }

//    const form = document.getElementById('selectStoreForm');
//    const formData = new FormData(form);

//    // Преобразуем selectedProduct в JSON и добавляем в FormData
//    formData.append('selectedArticleJson', JSON.stringify(selectedProduct));

//    fetch('/ADSIndex?handler=SelectedStore', {
//        method: 'POST',
//        body: formData,
//        headers: {
//            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
//        }
//    })
//        .then(response => response.json())
//        .then(data => {
//            // Обновляем данные о стоке и резервах
//            document.getElementById('artStock').value = data.stock;
//            document.getElementById('artReserve').value = data.reserve;
//        })
//        .catch(error => {
//            console.error('Ошибка при отправке формы:', error);
//        });
//}






