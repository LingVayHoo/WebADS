function selectProduct(moySkladId) {
    fetch(`/AdsSLM?handler=SelectProduct&moySkladId=${moySkladId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {

            // Заполняем поля на странице
            document.getElementById('productName').value = data.name;
            document.getElementById('article').value = data.article;

            // Загружаем изображение, если оно есть
            if (data.imageURL) {
                const productImage = document.getElementById('productImage');
                productImage.src = data.imageURL;
            }

            // Сохраняем в cookies
            setCookie('selectedProduct', JSON.stringify(data), 3);

            submitStoreForm();
            loadAddresses(data.article);

            // Показываем содержимое после загрузки данных
            showContent();
        })
        .catch(error => {
            console.error('Ошибка при загрузке данных:', error);
            document.getElementById('loadingMessage').innerText = 'Ошибка при загрузке данных. Пожалуйста, попробуйте снова.';
        });
}

function getParameters(moySkladId) {
    fetch(`/AdsSLM?handler=ArticleParams&moySkladId=${moySkladId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {

            const modalElement = document.getElementById('parametersModal');
            const modal = new bootstrap.Modal(modalElement);

            const productIDField = modalElement.querySelector('#productID');
            const awsField = modalElement.querySelector('#aws');
            const salesMethodField = modalElement.querySelector('#salesMethod');
            const minSalesQtyField = modalElement.querySelector('#minSalesQty');
            const multipackQtyField = modalElement.querySelector('#multipackQty');
            const palletQtyField = modalElement.querySelector('#palletQty');
            console.log(data);
            productIDField.value = data.productID;
            awsField.value = data.aws;
            salesMethodField.value = data.salesMethod;
            minSalesQtyField.value = data.minSalesQty;
            multipackQtyField.value = data.multipackQty;
            palletQtyField.value = data.palletQty;

            // Показываем модальное окно
            modal.show();

        })
        .catch(error => {
            console.error('Ошибка при загрузке данных:', error);
            document.getElementById('loadingMessage').innerText = 'Ошибка при загрузке данных. Пожалуйста, попробуйте снова.';
        });
}

function showConfirmModal(message) {
    return new Promise((resolve) => {
        const modal = document.getElementById('confirmModal');
        const modalMessage = document.getElementById('modalMessage');
        const modalConfirm = document.getElementById('modalConfirm');
        const modalCancel = document.getElementById('modalCancel');

        modalMessage.innerText = message;
        modal.style.display = 'flex'; // Показываем модальное окно

        modalConfirm.onclick = function () {
            modal.style.display = 'none';  // Закрываем модальное окно
            resolve(true);  // Возвращаем true, если подтверждено
        };

        modalCancel.onclick = function () {
            modal.style.display = 'none';  // Закрываем модальное окно
            resolve(false);  // Возвращаем false, если отмена
        };
    });
}

function showAlertModal(message) {
    return new Promise((resolve) => {
        const modal = document.getElementById('alertModal');
        const modalMessage = document.getElementById('alertmodalMessage');
        const modalConfirm = document.getElementById('alertmodalConfirm');

        modalMessage.innerText = message;
        modal.style.display = 'flex'; // Показываем модальное окно

        modalConfirm.onclick = function () {
            modal.style.display = 'none';  // Закрываем модальное окно
            resolve(true);  // Возвращаем true, если подтверждено
        };
    });
}

function setCookie(name, value, hours) {
    let expires = "";
    if (hours) {
        let date = new Date();
        date.setTime(date.getTime() + hours * 60 * 60 * 1000);
        expires = "; expires=" + date.toUTCString();
    }

    // Формируем строку cookie с SameSite и Secure (если на https)
    let cookie = name + "=" + encodeURIComponent(value) + expires + "; path=/; SameSite=Strict";
    if (location.protocol === "https:") {
        cookie += "; Secure";
    }
    document.cookie = cookie;
}

function getCookie(name) {
    let cookies = document.cookie.split('; ');
    for (let cookie of cookies) {
        let [key, value] = cookie.split('=');
        if (key === name) return decodeURIComponent(value);
    }
    return null;
}


function submitStoreForm() {
    const selectedProductCookie = getCookie('selectedProduct');

    if (!selectedProductCookie) {
        showAlertModal('Перелогинься, что-то пошло не так!');
        return;
    }

    const selectedProduct = JSON.parse(selectedProductCookie);

    const selectedStore = document.getElementById('store').value;

    const data = new URLSearchParams();
    data.append('SelectedStore', selectedStore);
    data.append('SelectedArticleJson', JSON.stringify(selectedProduct));

    fetch('/ADSSLM?handler=SelectedStore', {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: data
    })
        .then(response => response.json())
        .then(data => {
            document.getElementById('artStock').value = data.stock;
            document.getElementById('artReserve').value = data.reserve;
        })
        .catch(error => {
            console.error('Ошибка при отправке данных:', error);
        });

    loadAddresses(selectedProduct.article); // Перезагружаем адреса
}

// Функция для получения выбранного склада
function getSelectedStore() {
    // Получаем данные о складах из HTML-элемента
    const storesData = document.getElementById('stores-data').dataset.stores;
    const stores = JSON.parse(storesData); // Преобразуем JSON в массив значений

    const storeSelect = document.getElementById('store');
    const selectedKey = storeSelect.value; // Получаем ключ выбранного склада

    // Если выбрано "Все склады", возвращаем "all"
    if (selectedKey === "all") {
        return "all";
    }

    // Получаем значение (название склада) по ключу
    const selectedStore = stores[selectedKey];
    return selectedStore; // Возвращаем название склада
}

function loadAddresses(article) {
    // Получаем данные о складах из HTML-элемента
    const storesData = document.getElementById('stores-data').dataset.stores;
    const stores = JSON.parse(storesData); // Преобразуем JSON в массив значений

    // Получаем выбранный склад
    const selectedStore = getSelectedStore();

    fetch(`/ADSSLM?handler=GetAddresses&article=${article}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.json())
        .then(data => {
            const container = document.getElementById('addresses-container');
            if (!container) {
                console.error('Элемент addresses-container не найден!');
                return;
            }

            // Очистим контейнер, если нужно загрузить новые данные
            container.innerHTML = "";

            // Фильтруем адреса по выбранному складу
            const filteredAddresses = selectedStore === "all"
                ? data // Если selectedStore === "all", возвращаем все адреса
                : data.filter(address => address.storeID === selectedStore); // Иначе фильтруем

            filteredAddresses.forEach(address => {
                addAddressToTable(address);                
            });
        })
        .catch(error => console.error('Ошибка при получении адресов:', error));
}

function isNullOrEmpty(value) {
    return value === null || value === undefined || value === '';
}

// Функция для добавления адреса в таблицу
function addAddressToTable(address) {
    // Клонируем шаблон
    const template = document.getElementById('address-template');
    const clone = template.cloneNode(true); // Глубокое клонирование
    clone.id = ''; // Убираем id, чтобы избежать дублирования

    const productId = address.productID;

    if (isNullOrEmpty(productId)) {
        const selectedProductCookie = getCookie('selectedProduct');

        if (!selectedProductCookie) {
            showAlertModal('Перелогинься, что-то пошло не так!');
            return;
        }

        const selectedProduct = JSON.parse(selectedProductCookie);
        productId = selectedProduct.id;
        address.productID = productId;
    }

    // Заполняем данные
    const tableBody = clone.querySelector('#addressTableBody');
    const row = document.createElement('tr');
    row.innerHTML = `
    <td>${address.storeID}</td>
    <td>${address.zone}</td>
    <td>${address.row}-${address.place}-${address.level}</td>
    <td>${address.qty}</td>
    <td>${address.isSalesLocation ? "Да" : ""}</td>
    <td>${address.isPrimaryPlace ? "Да" : ""}</td>
    <td></td> <!-- Пустая ячейка для выравнивания -->
`;

    const buttonRow = document.createElement('tr');
    buttonRow.innerHTML = `
    <td colspan="7"> <!-- Объединяем ячейки для кнопок -->
        <div class="button-container">
            <button type="button" class="edit-address btn btn-primary">Редактировать</button>
            <button type="button" class="delete-address btn btn-danger">Удалить</button>
        </div>
    </td>
`;

    // Добавляем строку в таблицу
    tableBody.appendChild(row);
    tableBody.appendChild(buttonRow);

    // Показываем клонированный шаблон
    clone.style.display = 'block';

    // Добавляем клонированный шаблон в контейнер
    const container = document.getElementById('addresses-container');
    container.appendChild(clone);

    // Добавляем обработчики для кнопок
    const editButton = buttonRow.querySelector('.edit-address');
    const deleteButton = buttonRow.querySelector('.delete-address');

    editButton.addEventListener('click', () => {
        editAddress(address);
    });

    deleteButton.addEventListener('click', () => {
        deleteAddress(address);
    });
}

// Функция для генерации опций складов
function generateStoreOptions(selectedStoreID) {
    const storesData = document.getElementById('stores-data').dataset.stores;
    const stores = JSON.parse(storesData);

    return Object.entries(stores).map(([key, value]) => {
        const selected = value === selectedStoreID ? 'selected' : '';
        return `<option value="${value}" ${selected}>${value}</option>`;
    }).join('');
}

function editAddress(address) {
    // Получаем модальное окно
    const modalElement = document.getElementById('createAddressModal');
    const modal = new bootstrap.Modal(modalElement);

    // Получаем данные о складах из HTML-элемента
    const storesData = document.getElementById('stores-data').dataset.stores;
    const stores = JSON.parse(storesData); // Преобразуем JSON в массив значений

    // Получаем выпадающий список для StoreID
    const storeSelect = modalElement.querySelector('#storeID');

    // Очищаем список перед заполнением
    storeSelect.innerHTML = '';

    const storesArray = Object.values(stores);

    // Заполняем выпадающий список опциями
    storesArray.forEach(store => {
        const option = document.createElement('option');
        option.value = store; // Значение опции
        option.textContent = store; // Текст опции
        storeSelect.appendChild(option);
    });

    // Получаем поля внутри модального окна
    const addressId = modalElement.querySelector('#addressId');
    const productID = modalElement.querySelector('#productID');
    const articleField = modalElement.querySelector('#article');
    const nameField = modalElement.querySelector('#productName');
    const zoneField = modalElement.querySelector('#zone');
    const rowField = modalElement.querySelector('#row');
    const placeField = modalElement.querySelector('#place');
    const levelField = modalElement.querySelector('#level');
    const qtyField = modalElement.querySelector('#qty');
    const storeIDField = modalElement.querySelector('#storeID');
    const isPrimaryField = modalElement.querySelector('#isPrimaryPlace');
    const isSalesLocation = modalElement.querySelector('#isSalesLocation');

    // Получаем значения артикула и названия из внешних элементов
    const article = document.getElementById('article').value;
    const name = document.getElementById('productName').value;


    // Заполняем поля в модальном окне
    addressId.value = address.id;
    productID.value = address.productID;
    articleField.value = article;
    nameField.value = name;
    zoneField.value = address.zone;
    rowField.value = address.row;
    placeField.value = address.place;
    levelField.value = address.level;
    qtyField.value = address.qty;
    storeIDField.value = address.storeID;
    isPrimaryField.checked = address.isPrimaryPlace;
    isSalesLocation.checked = address.isSalesLocation;

    // Показываем модальное окно
    modal.show();

    const createButton = document.getElementById('confirmCreateAddress');
    createButton.style.display = 'none'; // Скрыть элемент

    const editButton = document.getElementById('confirmEditAddress');
    editButton.style.display = 'block'; // Показать элемент
}

// Функция для переключения режима редактирования
function toggleEditMode(addressDiv, editButton, deleteButton, isCancel) {
    const inputs = addressDiv.querySelectorAll('input[type="text"], input[type="checkbox"], select');
    const isEditing = editButton.textContent === "Редактировать";

    // Отключаем все кнопки на странице (если это необходимо)
    const allButtons = document.querySelectorAll('button');
    allButtons.forEach(button => {
        button.disabled = true;
    });

    // Активируем кнопку редактирования
    editButton.disabled = false;

    if (isEditing) {
        // Включаем редактирование
        inputs.forEach(input => {
            if (input.type === "checkbox") {
                input.disabled = false; // Активируем чекбоксы
            } else if (input.tagName === "SELECT") {
                input.disabled = false; // Активируем выпадающий список
            } else {
                input.readOnly = false; // Активируем текстовые поля
            }
        });

        // Меняем кнопку на "Подтвердить"
        editButton.textContent = "Подтвердить";
        editButton.classList.remove('btn-primary');
        editButton.classList.add('btn-success');

        // Меняем кнопку "Удалить" на "Cancel"
        deleteButton.textContent = "Cancel";
        deleteButton.classList.remove('btn-danger');
        deleteButton.classList.add('btn-secondary');
        deleteButton.disabled = false;

        // Добавляем обработчик для кнопки "Cancel"
        deleteButton.removeEventListener('click', handleDelete); // Удаляем старый обработчик
        deleteButton.addEventListener('click', handleCancel); // Добавляем новый обработчик

        // Добавляем класс для изменения фона
        addressDiv.classList.add('edit-mode');
    } else {

        // Отключаем редактирование
        inputs.forEach(input => {
            if (input.type === "checkbox") {
                input.disabled = true; // Деактивируем чекбоксы
            } else if (input.tagName === "SELECT") {
                input.disabled = true; // Деактивируем выпадающий список
            } else {
                input.readOnly = true; // Деактивируем текстовые поля
            }
        });

        // Меняем кнопку обратно на "Редактировать"
        editButton.textContent = "Редактировать";
        editButton.classList.remove('btn-success');
        editButton.classList.add('btn-primary');

        // Меняем кнопку "Cancel" обратно на "Удалить"
        deleteButton.textContent = "Удалить";
        deleteButton.classList.remove('btn-secondary');
        deleteButton.classList.add('btn-danger');
        deleteButton.disabled = false;

        // Убираем обработчик "Cancel" и возвращаем обработчик "Удалить"
        deleteButton.removeEventListener('click', handleCancel); // Удаляем обработчик "Cancel"
        deleteButton.addEventListener('click', handleDelete); // Возвращаем обработчик "Удалить"

        // Активируем все кнопки на странице
        allButtons.forEach(button => {
            button.disabled = false;
        });

        // Убираем класс для изменения фона
        addressDiv.classList.remove('edit-mode');

        // Отправляем данные на сервер
        if (!isCancel) {
            saveAddress(addressDiv);
        }
        else {
            const art = addressDiv.querySelector('input[name="Article"]').value;
            loadAddresses(art);
        }
    }
}

// Обработчик для кнопки "Cancel"
function handleCancel() {
    const addressDiv = this.closest('.address-block'); // Находим родительский блок адреса
    const editButton = addressDiv.querySelector('.edit-address');
    const deleteButton = addressDiv.querySelector('.delete-address');

    // Отключаем режим редактирования
    toggleEditMode(addressDiv, editButton, deleteButton, true);
}

// Обработчик для кнопки "Удалить"
function handleDelete() {
    const addressDiv = this.closest('.address-block'); // Находим родительский блок адреса
    deleteAddress(addressDiv); // Вызываем функцию удаления
}

// Функция для сохранения адреса
function saveAddress(modalElement) {
    // Собираем данные из блока адреса
    const art = modalElement.querySelector('input[name="Article"]').value;

    const addressHistory = {
        addressDBModel: { // Исправлено: используется :
            Id: modalElement.querySelector('#addressId').value,
            ProductID: modalElement.querySelector('#productID').value,
            Article: art,
            ProductName: modalElement.querySelector('#productName').value,
            Zone: modalElement.querySelector('#zone').value,
            Row: modalElement.querySelector('#row').value,
            Place: modalElement.querySelector('#place').value,
            Level: modalElement.querySelector('#level').value,
            Qty: modalElement.querySelector('#qty').value,
            StoreID: modalElement.querySelector('#storeID').value,
            IsPrimaryPlace: modalElement.querySelector('#isPrimaryPlace').checked,
            IsSalesLocation: modalElement.querySelector('#isSalesLocation').checked
        },
        ChangedBy: "" // Пустая строка или значение по умолчанию
    };

    // Отправляем данные на сервер
    fetch('/ADSSLM?handler=PutAddress', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(addressHistory)
    })
        .then(response => response.json())
        .then(updatedAddress => {
            
            const modal = bootstrap.Modal.getInstance(document.getElementById('createAddressModal'));
            clearModalFields();
            modal.hide();
            showAlertModal('Адрес успешно обновлен!');
            loadAddresses(art);
        })
        .catch(error => {
            console.error('Ошибка при обновлении адреса:', error);
            showAlertModal('Произошла ошибка при обновлении адреса.');
        });
}

async function deleteAddress(address) {
    const id = address.id;

    // Ждем, пока пользователь не подтвердит или не отменит удаление
    const confirmed = await showConfirmModal('Вы уверены, что хотите удалить этот адрес?');

    if (!confirmed) {
        return; // Отмена удаления
    }

    fetch('/ADSSLM?handler=DeleteAddress', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ guidId: id }) // Отправляем Id адреса
    })
        .then(response => response.json())
        .then(result => {
            if (result) {
                // Устанавливаем сообщение в TempData
                showAlertModal('Адрес успешно удален!');
                loadAddresses(address.article);
            } else {
                showAlertModal('Не удалось удалить адрес.');
            }
        })
        .catch(error => {
            console.error('Ошибка при удалении адреса:', error);
            showAlertModal('Произошла ошибка при удалении адреса.');
        });
}

// Функция для создания адреса
function createAddress() {
    const selectedProductCookie = getCookie('selectedProduct');

    if (!selectedProductCookie) {
        showAlertModal('Перелогинься, что-то пошло не так!');
        return;
    }

    const selectedProduct = JSON.parse(selectedProductCookie);


    const productID = selectedProduct.id;
    const productName = selectedProduct.name;
    const storeID = document.getElementById('storeID').value;
    const article = selectedProduct.article;
    const zone = document.getElementById('zone').value;
    const row = document.getElementById('row').value;
    const placeNumber = document.getElementById('place').value;
    const level = document.getElementById('level').value;
    const qty = document.getElementById('qty').value;
    const isPrimaryPlace = document.getElementById('isPrimaryPlace').checked;
    const isSalesLocation = document.getElementById('isSalesLocation').checked;

    // Формируем объект для отправки на сервер
    const addressData = {
        addressDBModel: {
            ProductID: productID,
            ProductName: productName,
            StoreID: storeID,
            Article: article,
            Zone: zone,
            Row: row,
            Place: placeNumber,
            Level: level,
            Qty: qty,
            IsPrimaryPlace: isPrimaryPlace,
            IsSalesLocation: isSalesLocation
        },
        ChangedBy: "" // Пустая строка или значение по умолчанию
    };

    // Отправляем данные на сервер
    fetch('/ADSSLM?handler=CreateAddress', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(addressData)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showAlertModal('Адрес успешно создан!');
                // Закрываем модальное окно
                const modal = bootstrap.Modal.getInstance(document.getElementById('createAddressModal'));
                clearModalFields();
                modal.hide();
                // Обновляем список адресов
                loadAddresses(article); // Перезагружаем адреса для текущего артикула
            } else {
                showAlertModal('Ошибка при создании адреса: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Ошибка:', error);
            showAlertModal('Произошла ошибка при создании адреса.');
        });
}

// Функция для очистки полей
function clearModalFields() {
    const modalElement = document.getElementById('createAddressModal');
    // Очищаем текстовые поля
    const inputs = modalElement.querySelectorAll('input[type="text"], input[type="number"], input[type="email"]');
    inputs.forEach(input => {
        input.value = '';
    });

    // Очищаем выпадающие списки (select)
    const selects = modalElement.querySelectorAll('select');
    selects.forEach(select => {
        select.selectedIndex = 0; // Устанавливаем первый вариант по умолчанию
    });

    // Очищаем чекбоксы и радиокнопки
    const checkboxes = modalElement.querySelectorAll('input[type="checkbox"], input[type="radio"]');
    checkboxes.forEach(checkbox => {
        checkbox.checked = false;
    });
}

// Функция для загрузки истории изменений по артикулу
async function loadHistoryByArticle(article) {
    try {
        const response = await fetch(`/ADSSLM?handler=GetHistoryByArticle&article=${article}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })

        if (!response.ok) {
            throw new Error('Ошибка при загрузке данных');
        }

        const data = await response.json();

        // Очищаем таблицу перед добавлением новых данных
        const tableBody = document.getElementById('historyTableBody');
        tableBody.innerHTML = '';



        // Добавляем каждую запись в таблицу
        data.forEach(record => {

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${record.changeType}</td>
                <td>${record.oldValuesFormatted}</td>
                <td>${record.newValuesFormatted}</td>
                <td>${new Date(record.changeDate).toLocaleString()}</td>
                <td>${record.changedBy}</td>
            `;
            tableBody.appendChild(row);
        });

        // Открываем модальное окно
        const modal = new bootstrap.Modal(document.getElementById('historyModal'));
        modal.show();
    } catch (error) {
        console.error('Ошибка:', error);
        showAlertModal('Не удалось загрузить данные.');
    }
}



