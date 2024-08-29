window.addEventListener('load', OnLoadCommon);

/***** onload() ******/
function OnLoadCommon() {

    $('.GridEvents').each(function () {

        theGrid = wijmo.Control.getControl("#" + this.id);

        // EVENTO DRAGGEDCOLUMN : PARA CONTROLAR Y SALVAR LAS GRIDS-COLUMNAS (ANCHO Y POSICION): cuando se mueven columnas...
        theGrid.draggedColumn.addHandler((s, e) => { /* s = thegrid */            
            N_Grid_Save_Config_Headers(s);
        });

        // EVENT PINNED
        theGrid.pinnedColumn.addHandler((s, e) => {
            N_Grid_Save_Config_Headers(s);
        });

        theGrid.resizedColumn.addHandler((s, e) => {
            N_Grid_Save_Config_Headers(s);
        });


        //theGrid.sortedColumn.addHandler((s, e) => {
        //    console.log("sortedColumn");
        //});

    });

    /**************************************************************************************************** */
    /* EVENTO DROP, MOVER FILES DENTRO DE UNA FLEXIGRID                                                   */
    /**************************************************************************************************** */
    var theGridArtMO = wijmo.Control.getControl("#gridArticuloMO");
    if (theGridArtMO != null) {

        theGridArtMO.hostElement.addEventListener("dragstart", function (e) {
            // Obtén el índice de la fila que se está moviendo
            var rowIndex = theGridArtMO.selection.row;

            // Almacena los datos de la fila en el objeto de transferencia de datos
            e.dataTransfer.setData("text", JSON.stringify(theGridArtMO.rows[rowIndex].dataItem));
        });

        theGridArtMO.hostElement.addEventListener("drop", function (e) {
            e.preventDefault();

            // Recupera los datos de la fila movida del objeto de transferencia de datos
            var datosFilaMovida = JSON.parse(e.dataTransfer.getData("text"));

            // Realiza las operaciones que necesites con los datos de la fila movida
            var MoveEmpresa = datosFilaMovida.Empresa;
            var MoveArticulo = datosFilaMovida.Articulo;
            var MoveGuid = datosFilaMovida.Guid;
            /*var NuevaPos = theGridArtMO.hitTest(e.clientX, e.clientY).row;*/
            var NuevaPos = theGridArtMO.hitTest(e.pageX, e.pageY).row;

            if (NuevaPos == -1) {
                NuevaPos = 0;
            }


            var formData = new FormData();
            formData.append('MoveEmpresa', MoveEmpresa);
            formData.append('MoveArticulo', MoveArticulo);
            formData.append('MoveGuid', MoveGuid);
            formData.append('NuevaPos', NuevaPos);

            fetch('/Articulos/ManoObraReordenar', {
                method: 'POST',
                //headers: {
                // 'Content-Type': 'application/json',
                //},
                body: formData
                //body: JSON.stringify(parametros)
            })
                .then(response => {
                    if (response.ok) {

                        return response.json();
                    } else {
                        console.error(`Error del servidor: ${response.status} ${response.statusText}`);
                        return response.text().then(text => { throw new Error(text) });
                    }
                })
                .then(data => {
                    //console.log('Éxito:', data);
                })
                .catch((error) => {
                    console.error('Error:', error);
                });

        }, true);

    }


    /**************************************************************************************************** */


    theSearch = wijmo.Control.getControl("#gridSearch");
    if (theSearch) {

        theSearch.hostElement.addEventListener("input", function (e) {
            if (theSearch.text == "") {
                setTimeout(function () {
                    theSearch._rxSearch = null;
                    theSearch._rxHighlight = null;
                    theSearch._view.refresh();
                }, theSearch.delay);
            }
        }, true);
        theSearch.hostElement.addEventListener("mousedown", function (e) {
            if (e.target.tagName == "BUTTON") {
                setTimeout(function () {
                    theSearch._rxSearch = null;
                    theSearch._rxHighlight = null;
                    theSearch._view.refresh();
                }, theSearch.delay);
            }
        }, true);
    }



}




function OpenQuery(theGrid, data) {

    // SE GUARDA CONFIG. DE GRID (FILTRO,ORDEN, COLUMNAS)
    var theFilter = c1.getExtender(theGrid, "theGridFilter");

    var sorts = theGrid.collectionView.sortDescriptions.map(function (sort) {
        return { property: sort.property, ascending: sort.ascending };
    });

    var state = {
        columns: theGrid.columnLayout,
        filterDefinition: theFilter.filterDefinition,
        sortDescriptions: sorts
    };

    // CARGA DE DATOS - ITEMSOURCE data
    var view = new wijmo.collections.CollectionView(data);
    theGrid.itemsSource = view;


    // RESTORE CONFIG
    theGrid.columnLayout = state.columns;

    theFilter.filterDefinition = state.filterDefinition;

    theGrid.collectionView.sortDescriptions.clear();
    for (var i = 0; i < state.sortDescriptions.length; i++) {
        var sortDesc = state.sortDescriptions[i];
        theGrid.collectionView.sortDescriptions.push(
            new wijmo.collections.SortDescription(sortDesc.property, sortDesc.ascending)
        );
    }

}

function N_Grid_Data_Refresh_List(theGrid, data) {

    for (let i = 0; i < data.length; i++) {
        var itemData = data[i];

        var indexGrid = theGrid.itemsSource.sourceCollection.findIndex(s => s.Guid == itemData.Guid);
        if (indexGrid != -1) {
            //console.log("indexGrid: " + indexGrid);
            //console.log(theGrid.itemsSource.sourceCollection[indexGrid]);

            var CurrentItem = theGrid.itemsSource.sourceCollection[indexGrid];

            Object.keys(itemData).forEach(function (key) {
                CurrentItem[key] = itemData[key];
            })

        }   
    }    

    theGrid.collectionView.refresh();
}

function N_Grid_Data_Refresh(theGrid, data) {

    var CurrentItem = theGrid.collectionView.currentItem;

    Object.keys(data).forEach(function (key) {
        CurrentItem[key] = data[key];
    })

    theGrid.collectionView.refresh();
}

function N_Grid_Data_Add(theGrid, data) {

    let existingItemIndex = theGrid.collectionView.sourceCollection.findIndex(item => item.Guid === data.Guid);

    if (existingItemIndex !== -1) {
        // Actualiza el elemento existente
        let itemToUpdate = theGrid.collectionView.sourceCollection[existingItemIndex];
        Object.assign(itemToUpdate, data);

    } else {
        // Agrega un nuevo elemento si no existe
        theGrid.collectionView.sourceCollection.push(data);

    }
    theGrid.collectionView.refresh();

    // Select row in grid
    var rows = theGrid.rows;
    var index = rows.findIndex(s => s.dataItem.Guid == data.Guid)
    theGrid.select(index, 1);
}


function N_Grid_Data_Delete(theGrid, Guid) {
    var index = theGrid.itemsSource.sourceCollection.findIndex(s => s.Guid == Guid);
    if (index != -1) {
        theGrid.itemsSource.sourceCollection.splice(index, 1); // 2nd parameter means remove one item only
        theGrid.collectionView.refresh();
        theGrid.select(-1, -1);
    }        
}

function N_Grid_Reposition(theGrid, Guid) {

    // Select row in grid
    var rows    = theGrid.rows;
    var index = rows.findIndex(s => s.dataItem.Guid == Guid);
    theGrid.scrollIntoView(index + 14, 1); // + 14 para que se posicione por la mitad aprox. de la grid
    theGrid.select(index , 0);
    

}

function N_Grid_Save_Config_Headers(theGrid) {

    let GridIdToSave    = theGrid._orgAtts.id.nodeValue;
    let ColumnsToSave   = theGrid.columnLayout;


    let ColumnsPinned = 0;
    for (let i = 1; i < theGrid.columns.length; i++) {
        if (theGrid.columns.isFrozen(i) == false) {
            ColumnsPinned = i;
            break;
        }
    }

    var data = new FormData();
    data.append("GridIdToSave",     GridIdToSave);
    data.append("ColumnsToSave",    ColumnsToSave);
    data.append("ColumnsPinned",    ColumnsPinned);

    $.ajax({
        type: 'POST',
        url: '/FunctionsBBDD/GridConfigSave',
        async: true,
        data: data,
        processData: false,
        contentType: false,
        success: function (data) {

        },
        always: function (data) { },
        error: function (xhr, status, error) {
            console.log("Error", xhr, xhr.responseText, status, error);
        }
    });

}

function N_Grid_Reset_Config_Headers(theGrid) {


    let GridIdToSave    = theGrid._orgAtts.id.nodeValue;

    var data = new FormData();
    data.append("GridIdToSave", GridIdToSave);

    $.ajax({
        type: 'POST',
        url: '/FunctionsBBDD/GridConfigReset',
        async: true,
        data: data,
        processData: false,
        contentType: false,
        success: function (data) {

            location.reload();

        },
        always: function (data) { },
        error: function (xhr, status, error) {
            console.log("Error", xhr, xhr.responseText, status, error);
        }
    });

}


///////////////////// FUNCIONES QUE SE ENCARGAN DE TRATAR LAS CELDAS EN LAS GRIDS Y EL TOOLTIP ////////////////////////////////////////////////


function onFormatItem(s, e) {
    if (e.panel == s.cells) {
        var col = s.columns[e.col];
        var content = s.getCellData(e.row, e.col, true);
        if (content && typeof content === 'string') {
            if (e.cell.scrollWidth > e.cell.offsetWidth) {
                e.cell.title = content;
            } else {
                e.cell.title = '';
            }
        }
    }
}


function setupTooltips(grid) {
    if (grid) {
        grid.invalidate();
    }
}

function initializeGrid(gridId, searchId, searchFields) {
    //console.log("Initializing grid:", gridId);
    var grid = wijmo.Control.getControl("#" + gridId);
    //console.log('Grid object:', grid);
    var search = wijmo.Control.getControl("#" + searchId);

    if (grid) {
        grid.formatItem.addHandler(onFormatItem);
        grid.select(-1, -1);
        setupTooltips(grid);
    }

    search.hostElement.addEventListener("input", function (e) {
        if (search.text != "") {
            applyFilters(grid, search, searchFields);
            if (typeof SetCookiesView === 'function') {
                SetCookiesView();
            }
        }
    }, true);

    search.hostElement.addEventListener("mousedown", function (e) {
        if (e.target.tagName == "BUTTON") {
            var Cook_Busqueda = getCookie("Cook_" + gridId + "_Busqueda");
            if (Cook_Busqueda != "") {
                setCookie("Cook_" + gridId + "_Busqueda", "");
            }
            search.text = "";
            applyFilters(grid, search, searchFields);  // Asegúrate de pasar searchFields aquí

            setTimeout(function () {
                search._rxSearch = null;
                search._rxHighlight = null;
                search._view.refresh();
            }, search.delay);
        }
    }, true);

    return { grid: grid, search: search };
}


function applyFilters(grid, search, searchFields) {
    //console.log('Applying filters:', { grid, search, searchFields });

    if (!searchFields || !Array.isArray(searchFields) || searchFields.length === 0) {
        console.warn('searchFields no está definido o está vacío');
        grid.collectionView.filter = null;
        return;
    }

    grid.collectionView.filter = function (item) {
        return searchFields.some(field =>
            item[field] && item[field].toString().toLowerCase().includes(search.text.toLowerCase())
        );
    };
}

function CargaDatos(grid, callback) {
    var Spinner = Rats.UI.LoadAnimation.start();

    if (grid && grid.collectionView) {
        grid.collectionView.refresh();
        setupTooltips(grid);
        Spinner.stop();
        if(callback) callback();
    } else {
        console.error("El grid no está inicializado correctamente");
        Spinner.stop();
    }
}

function BtnResetConfigGrid(grid) {
    N_Grid_Reset_Config_Headers(grid);
    setupTooltips(grid);
}


function setupColumnResizeIndicator(grid) {
    let indicator = document.createElement('div');
    indicator.className = 'column-resize-indicator';
    document.body.appendChild(indicator);

    grid.addEventListener(grid.hostElement, 'mousemove', function (e) {
        let ht = grid.hitTest(e);
        if (ht.cellType == wijmo.grid.CellType.ColumnHeader && ht.edgeRight) {
            let rc = grid.getCellBoundingRect(ht.row, ht.col);
            let gridRect = grid.hostElement.getBoundingClientRect();
            // Calcular la altura basada en el número de filas visibles
            let headerHeight = grid.columnHeaders.rows.length * grid.rows.defaultSize;
            let visibleRows = grid.rows.filter(function (r) { return r.visible; });
            let cellsHeight = visibleRows.length * grid.rows.defaultSize;
            let totalHeight = headerHeight + cellsHeight;
            // Asegurarse de que la altura no exceda el tamaño visible de la grid
            let maxHeight = gridRect.height;
            totalHeight = Math.min(totalHeight, maxHeight);
            indicator.style.display = 'block';
            indicator.style.left = (rc.right + gridRect.left - 113) + 'px';
            indicator.style.top = gridRect.top + 'px';
            indicator.style.height = totalHeight + 'px';
            indicator.style.zIndex = '1000';
        } else {
            indicator.style.display = 'none';
        }
    });
    grid.addEventListener(grid.hostElement, 'mouseleave', function () {
        indicator.style.display = 'none';
    });
}