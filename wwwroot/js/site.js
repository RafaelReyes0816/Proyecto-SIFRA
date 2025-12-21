// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// SOLUCIÓN ULTRA AGRESIVA: Eliminar COMPLETAMENTE los backdrops usando MutationObserver
(function() {
    // Función para eliminar TODOS los backdrops
    function eliminarTodosLosBackdrops() {
        var backdrops = document.querySelectorAll('.modal-backdrop');
        backdrops.forEach(function(backdrop) {
            backdrop.style.display = 'none';
            backdrop.style.visibility = 'hidden';
            backdrop.style.opacity = '0';
            backdrop.style.pointerEvents = 'none';
            backdrop.remove();
        });
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
        document.body.style.position = '';
    }
    
    // Usar MutationObserver para detectar cuando se crea un backdrop y eliminarlo INMEDIATAMENTE
    var observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            mutation.addedNodes.forEach(function(node) {
                if (node.nodeType === 1) { // Element node
                    if (node.classList && node.classList.contains('modal-backdrop')) {
                        node.remove();
                    }
                    // También buscar dentro del nodo
                    var backdrops = node.querySelectorAll ? node.querySelectorAll('.modal-backdrop') : [];
                    backdrops.forEach(function(backdrop) {
                        backdrop.remove();
                    });
                }
            });
        });
        // Limpiar cualquier backdrop que exista
        eliminarTodosLosBackdrops();
    });
    
    // Observar cambios en el body
    if (document.body) {
        observer.observe(document.body, {
            childList: true,
            subtree: true
        });
    } else {
        document.addEventListener('DOMContentLoaded', function() {
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        });
    }
    
    // Limpiar inmediatamente
    eliminarTodosLosBackdrops();
    
    // Limpiar al cargar la página
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', eliminarTodosLosBackdrops);
    } else {
        eliminarTodosLosBackdrops();
    }
    
    // Limpiar cuando se cierra cualquier modal
    document.addEventListener('hidden.bs.modal', eliminarTodosLosBackdrops);
    document.addEventListener('hide.bs.modal', function() {
        setTimeout(eliminarTodosLosBackdrops, 10);
    });
    
    // Limpiar cada 50ms - ULTRA AGRESIVO
    setInterval(eliminarTodosLosBackdrops, 50);
    
    // Interceptar el método _setEscapeEvent de Bootstrap para prevenir backdrop
    if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
        var originalConstructor = bootstrap.Modal;
        bootstrap.Modal = function(element, config) {
            if (config && config.backdrop !== false) {
                config.backdrop = false;
            }
            return new originalConstructor(element, config || { backdrop: false });
        };
        // Copiar propiedades estáticas
        Object.setPrototypeOf(bootstrap.Modal, originalConstructor);
        Object.assign(bootstrap.Modal, originalConstructor);
    }
})();

// Manejo de errores de validación - Mostrar modal y mensajes debajo de campos
document.addEventListener('DOMContentLoaded', function() {
    // Ocultar completamente todos los validation-summary
    var validationSummaries = document.querySelectorAll('div[asp-validation-summary]');
    validationSummaries.forEach(function(summary) {
        summary.classList.add('d-none');
        summary.style.display = 'none';
    });
    
    // Verificar si hay errores de ViewBag.Error (almacenados en div oculto)
    var errorMessages = [];
    var viewBagError = document.getElementById('viewBagError');
    if (viewBagError && viewBagError.getAttribute('data-error')) {
        errorMessages.push(viewBagError.getAttribute('data-error'));
    }
    
    // Verificar errores de ModelState (en validation-summary)
    var modelOnlySummaries = document.querySelectorAll('div[asp-validation-summary="ModelOnly"]');
    modelOnlySummaries.forEach(function(summary) {
        var errorText = summary.textContent.trim();
        if (errorText) {
            // Extraer mensajes de error de los <li> dentro del summary
            var listItems = summary.querySelectorAll('li');
            if (listItems.length > 0) {
                listItems.forEach(function(li) {
                    var text = li.textContent.trim();
                    if (text) {
                        errorMessages.push(text);
                    }
                });
            } else if (errorText) {
                errorMessages.push(errorText);
            }
        }
    });
    
    // Si hay errores generales, mostrar modal
    if (errorMessages.length > 0) {
        var errorModalElement = document.getElementById('errorModal');
        if (errorModalElement) {
            var errorModal = new bootstrap.Modal(errorModalElement);
            var errorModalMessage = document.getElementById('errorModalMessage');
            var errorModalList = document.getElementById('errorModalList');
            
            if (errorModalMessage && errorModalList) {
                if (errorMessages.length === 1) {
                    errorModalMessage.textContent = errorMessages[0];
                    errorModalMessage.style.marginBottom = '0';
                    errorModalList.innerHTML = '';
                    errorModalList.style.display = 'none';
                } else {
                    errorModalMessage.textContent = 'Se encontraron los siguientes errores:';
                    errorModalMessage.style.marginBottom = '1rem';
                    errorModalList.innerHTML = '';
                    errorModalList.style.display = 'block';
                    errorMessages.forEach(function(error) {
                        var li = document.createElement('li');
                        li.style.cssText = 'padding: 0.75rem; margin-bottom: 0.5rem; background: rgba(220, 53, 69, 0.15); border-left: 3px solid #dc3545; border-radius: 4px; color: #ffffff;';
                        li.textContent = '• ' + error;
                        errorModalList.appendChild(li);
                    });
                }
                errorModal.show();
            }
        }
    }
    
    // Asegurar que los mensajes de validación se muestren debajo de cada campo
    // Esperar a que jQuery Validation se inicialice
    setTimeout(function() {
        // Buscar todos los spans de validación (diferentes formas que usa ASP.NET)
        var validationSpans = document.querySelectorAll('span.field-validation-error, span[data-valmsg-for], span.text-danger');
        validationSpans.forEach(function(validationSpan) {
            var errorText = validationSpan.textContent.trim();
            
            // Buscar el input asociado en el mismo contenedor
            var group = validationSpan.closest('.mb-3, .form-group, .col-md-6, .col-md-12, .row > div');
            var input = null;
            
            if (group) {
                // Buscar input, select o textarea en el grupo
                input = group.querySelector('input, select, textarea');
            }
            
            // Si no se encuentra, buscar por el atributo data-valmsg-for
            if (!input) {
                var fieldName = validationSpan.getAttribute('data-valmsg-for');
                if (fieldName) {
                    // Buscar por name o id
                    input = document.querySelector('input[name="' + fieldName + '"], select[name="' + fieldName + '"], textarea[name="' + fieldName + '"], #' + fieldName);
                }
            }
            
            if (errorText && errorText.length > 0) {
                // Mostrar el mensaje de error
                validationSpan.style.display = 'block';
                validationSpan.style.color = '#dc3545';
                validationSpan.style.fontSize = '0.875rem';
                validationSpan.style.marginTop = '0.25rem';
                validationSpan.style.fontWeight = '500';
                
                // Agregar clase de error al input si existe
                if (input) {
                    input.classList.add('is-invalid');
                    input.classList.remove('is-valid');
                }
            } else {
                // Si no hay error, ocultar el span
                validationSpan.style.display = 'none';
                if (input) {
                    input.classList.remove('is-invalid');
                }
            }
        });
    }, 100);
    
    // Limpiar errores cuando el usuario empiece a escribir
    var inputs = document.querySelectorAll('input, select, textarea');
    inputs.forEach(function(input) {
        input.addEventListener('input', function() {
            var group = this.closest('.mb-3, .form-group, .col-md-6, .col-md-12');
            if (group) {
                var validationSpan = group.querySelector('span[data-valmsg-for], span.field-validation-error, span.text-danger');
                if (validationSpan) {
                    // Si el campo es válido, limpiar el error
                    if (this.checkValidity && this.checkValidity() && this.value.trim() !== '') {
                        validationSpan.textContent = '';
                        validationSpan.style.display = 'none';
                        this.classList.remove('is-invalid');
                        this.classList.add('is-valid');
                    }
                }
            }
        });
        
        // Validar al perder el foco
        input.addEventListener('blur', function() {
            if (this.checkValidity) {
                if (!this.checkValidity()) {
                    this.classList.add('is-invalid');
                    this.classList.remove('is-valid');
                } else if (this.value.trim() !== '') {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                }
            }
        });
    });
    
    // Manejar errores cuando se envía el formulario
    var forms = document.querySelectorAll('form');
    forms.forEach(function(form) {
        form.addEventListener('submit', function(e) {
            // Si hay errores de validación, mostrarlos
            var hasErrors = false;
            var formErrors = [];
            
            // Verificar errores en los spans
            var validationSpans = form.querySelectorAll('span[data-valmsg-for], span.field-validation-error, span.text-danger');
            validationSpans.forEach(function(span) {
                if (span.textContent.trim()) {
                    hasErrors = true;
                }
            });
            
            // Si hay errores, asegurar que se muestren
            if (hasErrors) {
                validationSpans.forEach(function(span) {
                    if (span.textContent.trim()) {
                        span.style.display = 'block';
                        var input = span.closest('.mb-3, .form-group')?.querySelector('input, select, textarea');
                        if (input) {
                            input.classList.add('is-invalid');
                        }
                    }
                });
            }
        });
    });
});
