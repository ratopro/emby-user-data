define(['emby-input', 'emby-button', 'emby-select', 'emby-checkbox'], function () {
    'use strict';
    return function (view) {
        var page = view;
        var fields = [], users = [], editing = null, selectedUser = null;
        function qs(selector) { return page.querySelector(selector); }
        function esc(value) { var node = document.createElement('div'); node.textContent = value == null ? '' : value; return node.innerHTML; }
        function note(message, error) { var el = qs('#notice'); el.textContent = message; el.className = 'ud-notice' + (error ? ' ud-danger' : ''); }
        function api(url, method, data) {
            return fetch(ApiClient.getUrl(url), {
                method: method || 'GET',
                headers: { 'X-Emby-Token': ApiClient.accessToken(), 'Content-Type': 'application/json' },
                body: data ? JSON.stringify(data) : undefined
            }).then(function (response) {
                if (!response.ok) {
                    return response.text().then(function (text) {
                        var error = { status: response.status, message: text };
                        try { error.responseJSON = JSON.parse(text); } catch (_) { }
                        throw error;
                    });
                }
                if (response.status === 204) { return null; }
                return response.text().then(function (text) { return text ? JSON.parse(text) : null; });
            });
        }
        function errorText(error, fallback) { var detail = error && error.responseJSON && (error.responseJSON.Message || error.responseJSON.message); var status = error && (error.status || error.statusCode); return (detail || (error && error.message) || fallback) + (status ? ' (HTTP ' + status + ')' : ''); }
        function showEditor(user) { selectedUser = user; qs('#userDirectory').classList.add('ud-hidden'); qs('#userEditor').classList.remove('ud-hidden'); qs('#selectedUserHeading').innerHTML = avatar(user) + '<strong>' + esc(user.Name) + '</strong>'; renderForm(); }
        function avatar(user) { var initial = esc((user.Name || '?').charAt(0).toUpperCase()); var src = ApiClient.getUrl('Users/' + user.Id + '/Images/Primary?width=96'); return '<img class="ud-avatar" src="' + src + '" alt="" onerror="this.outerHTML=\'<span class=\\\'ud-avatar\\\'>' + initial + '</span>\'">'; }
        function renderUsers() { var grid = qs('#userGrid'); grid.innerHTML = users.map(function (user) { return '<button class="ud-user" type="button" data-user="' + user.Id + '">' + avatar(user) + '<span><span class="ud-user-name">' + esc(user.Name) + '</span></span></button>'; }).join('') || '<p class="muted">No hay usuarios disponibles.</p>'; grid.querySelectorAll('[data-user]').forEach(function (button) { button.onclick = function () { showEditor(users.filter(function (user) { return user.Id === button.dataset.user; })[0]); }; }); }
        function loadUsers() { if (!ApiClient.getUsers) { note('Esta versión de Emby no expone la lista de usuarios al plugin.', true); return; } ApiClient.getUsers().then(function (result) { users = result.Items || result || []; renderUsers(); }).catch(function (error) { console.error('User Data: error al cargar usuarios', error); note(errorText(error, 'No se pudieron cargar los usuarios.'), true); }); }
        function addOption(value) {
            var row = document.createElement('div'); row.className = 'ud-option-row';
            row.innerHTML = '<input is="emby-input" type="text" class="ud-option-value" placeholder="Opción" value="' + esc(value || '') + '"><button is="emby-button" type="button" class="raised ud-option-delete" title="Eliminar opción">🗑</button>';
            row.querySelector('.ud-option-delete').onclick = function () { row.remove(); if (!qs('#fieldOptionsList').children.length) { addOption(''); } };
            qs('#fieldOptionsList').appendChild(row);
        }
        function setOptions(values) { qs('#fieldOptionsList').innerHTML = ''; (values && values.length ? values : ['']).forEach(addOption); }
        function getOptions() { return Array.prototype.slice.call(page.querySelectorAll('.ud-option-value')).map(function (input) { return input.value.trim(); }).filter(Boolean); }
        function resetField() { editing = null; qs('#fieldTitle').textContent = 'Nuevo campo'; qs('#fieldName').value = ''; qs('#fieldType').value = 'Text'; setOptions([]); qs('#optionsWrap').classList.add('ud-hidden'); }
        function saveFieldOrder() { return api('/UserData/Fields/Order', 'POST', { Ids: fields.map(function (field) { return field.Id; }) }).then(loadFields); }
        function renderFields() { var list = qs('#fieldList'); list.innerHTML = fields.map(function (field, index) { return '<li><span class="grow"><strong>' + esc(field.Name) + '</strong> <span class="ud-pill">' + esc(field.Type) + '</span> ' + (!field.IsActive ? '<span class="ud-danger">inactivo</span>' : '') + '</span><button data-move="up" data-id="' + field.Id + '" is="emby-button" class="ud-order-button" title="Subir campo" ' + (index === 0 ? 'disabled' : '') + '>▲</button><button data-move="down" data-id="' + field.Id + '" is="emby-button" class="ud-order-button" title="Bajar campo" ' + (index === fields.length - 1 ? 'disabled' : '') + '>▼</button><button data-edit="' + field.Id + '" is="emby-button">Editar</button><button data-toggle="' + field.Id + '" is="emby-button">' + (field.IsActive ? 'Desactivar' : 'Activar') + '</button><button data-delete="' + field.Id + '" is="emby-button" class="ud-danger">Borrar</button></li>'; }).join('') || '<li class="muted">Aún no hay campos.</li>';
            list.querySelectorAll('[data-move]').forEach(function (button) { button.onclick = function () { var index = fields.map(function (field) { return field.Id; }).indexOf(button.dataset.id); var next = button.dataset.move === 'up' ? index - 1 : index + 1; if (next < 0 || next >= fields.length) { return; } var moved = fields[index]; fields[index] = fields[next]; fields[next] = moved; saveFieldOrder().catch(function (error) { note(errorText(error, 'No se pudo guardar el orden.'), true); loadFields(); }); }; });
            list.querySelectorAll('[data-edit]').forEach(function (button) { button.onclick = function () { editing = fields.filter(function (field) { return field.Id === button.dataset.edit; })[0]; qs('#fieldTitle').textContent = 'Editar campo'; qs('#fieldName').value = editing.Name; qs('#fieldType').value = editing.Type; setOptions(editing.Options || []); qs('#optionsWrap').classList.toggle('ud-hidden', editing.Type !== 'Select'); }; });
            list.querySelectorAll('[data-toggle]').forEach(function (button) { button.onclick = function () { var field = fields.filter(function (item) { return item.Id === button.dataset.toggle; })[0]; field.IsActive = !field.IsActive; api('/UserData/Fields', 'POST', { Field: field }).then(loadFields); }; });
            list.querySelectorAll('[data-delete]').forEach(function (button) { button.onclick = function () { if (confirm('¿Borrar permanentemente este campo y todos sus valores?')) { api('/UserData/Fields/' + button.dataset.delete + '?Permanent=true', 'DELETE').then(loadFields); } }; }); }
        function loadFields() { return api('/UserData/Fields').then(function (result) { fields = result || []; renderFields(); if (selectedUser) { renderForm(); } }).catch(function (error) { console.error('User Data: error al cargar campos', error); note(errorText(error, 'No se pudieron cargar los campos.'), true); }); }
        function renderForm() { var form = qs('#valueForm'); if (!selectedUser) { return; } var active = fields.filter(function (field) { return field.IsActive; }); api('/UserData/Users/' + selectedUser.Id).then(function (record) { var values = record.Values || {}; form.innerHTML = '<h2>Datos adicionales</h2>' + active.map(function (field) { var value = values[field.Id] || ''; var control = '<label class="ud-label">' + esc(field.Name) + (field.Required ? ' *' : '') + '</label>'; if (field.Type === 'TextArea') { control += '<textarea data-field="' + field.Id + '" is="emby-input">' + esc(value) + '</textarea>'; } else if (field.Type === 'Select') { control += '<select data-field="' + field.Id + '" is="emby-select">' + (field.Options || []).map(function (option) { return '<option ' + (value === option ? 'selected' : '') + '>' + esc(option) + '</option>'; }).join('') + '</select>'; } else if (field.Type === 'Boolean') { control += '<label class="ud-switch" title="' + (value === 'true' ? 'Sí' : 'No') + '"><input data-field="' + field.Id + '" type="checkbox" ' + (value === 'true' ? 'checked' : '') + '><span class="ud-switch-slider"></span></label>'; } else { control += '<input data-field="' + field.Id + '" type="' + (field.Type === 'Date' ? 'date' : field.Type === 'Number' ? 'number' : 'text') + '" value="' + esc(value) + '" is="emby-input">'; } return '<div>' + control + '</div>'; }).join('') + '<div class="ud-actions"><button is="emby-button" class="raised button-submit">Guardar datos</button></div>';
                form.onsubmit = function (event) { event.preventDefault(); var valuesToSave = {}; form.querySelectorAll('[data-field]').forEach(function (element) { valuesToSave[element.dataset.field] = element.type === 'checkbox' ? String(element.checked) : element.value; }); api('/UserData/Users/' + selectedUser.Id, 'POST', { UserId: selectedUser.Id, Values: valuesToSave }).then(function () { note('Datos guardados.'); }).catch(function (error) { note(error.responseJSON && error.responseJSON.Message || 'No se pudieron guardar los datos.', true); }); }; }); }
        page.querySelectorAll('.ud-tabs button').forEach(function (button) { button.onclick = function () { page.querySelectorAll('.ud-tabs button,.ud-panel').forEach(function (element) { element.classList.remove('active'); }); button.classList.add('active'); qs('#' + button.dataset.tab).classList.add('active'); }; });
        qs('#fieldType').onchange = function () { qs('#optionsWrap').classList.toggle('ud-hidden', this.value !== 'Select'); };
        qs('#saveField').onclick = function () { var field = editing || {}; field.Name = qs('#fieldName').value; field.Type = qs('#fieldType').value; field.Required = false; field.IsActive = editing ? editing.IsActive : true; field.Options = getOptions(); api('/UserData/Fields', 'POST', { Field: field }).then(function () { note('Campo guardado.'); resetField(); loadFields(); }).catch(function (error) { console.error('User Data: error al guardar campo', error); note(errorText(error, 'No se pudo guardar el campo.'), true); }); };
        qs('#cancelField').onclick = resetField; qs('#addOption').onclick = function () { addOption(''); }; qs('#backToUsers').onclick = function () { selectedUser = null; qs('#userEditor').classList.add('ud-hidden'); qs('#userDirectory').classList.remove('ud-hidden'); };
        if (page.dataset.defaultTab === 'values') { page.querySelector('.ud-tabs button[data-tab="values"]').click(); }
                resetField(); loadUsers(); loadFields();
    };
});
