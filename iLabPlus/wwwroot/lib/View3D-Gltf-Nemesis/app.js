
////import WebGL from 'three/examples/jsm/capabilities/WebGL.js';
//import { WebGL } from 'https://cdn.skypack.dev/three@0.129.0/examples/jsm/capabilities/WebGL.js';

import { Viewer } from './viewer.js';

//import { SimpleDropzone } from 'simple-dropzone';
//import { SimpleDropzone } from './simple-dropzone.min.js';

//import { ValidationController } from './validation-controller.js';
//import queryString from 'query-string';

if (!(window.File && window.FileReader && window.FileList && window.Blob)) {
  console.error('The File APIs are not fully supported in this browser.');
}
//else if (!WebGL.isWebGLAvailable()) {
//  console.error('WebGL is not supported in this browser.');
//}

export class App {

    
  /**
   * @param  {Element} el
   * @param  {Location} location
   */
  constructor (el, location) {

    const hash = location.hash ? queryString.parse(location.hash) : {};
    this.options = {
      kiosk: Boolean(hash.kiosk),
      model: hash.model || '',
      preset: hash.preset || '',
      cameraPosition: hash.cameraPosition
        ? hash.cameraPosition.split(',').map(Number)
        : null
    };


    this.el = el;
    this.viewer = null;
    this.viewerEl = null;
    this.spinnerEl = el.querySelector('.spinner');
      this.dropEl = el.querySelector('.View3DWrapper');
    this.inputEl = el.querySelector('#file-input');
    //this.validationCtrl = new ValidationController(el);

    this.createDropzone(this);
    //this.hideSpinner();

    const options = this.options;

    if (options.kiosk) {
      const headerEl = document.querySelector('header');
      headerEl.style.display = 'none';
    }

    if (options.model) {
      this.view(options.model, '', new Map());
    }
  }

  /**
   * Sets up the drag-and-drop controller.
   */

  createDropzone (app) {
    //const dropCtrl = new SimpleDropzone(this.dropEl, this.inputEl);
    //dropCtrl.on('drop', ({files}) => this.load(files));
    //dropCtrl.on('dropstart', () => this.showSpinner());
    //dropCtrl.on('droperror', () => this.hideSpinner());


      
      //let imageUpload = document.getElementById("imageUpload");

      //imageUpload.onchange = function () {

      //      let input = this.files[0];
      //      console.log(input);

      //      const fileMap = new Map();
      //      fileMap.set('FicRutaPath', input);
      //      console.log(fileMap);


      //      let rootFile;
      //      let rootPath;

      //      Array.from(fileMap).forEach(([path, file]) => {
      //          console.log(file.name);

      //          if (file.name.match(/\.(gltf|glb)$/)) {
      //              rootFile = file;
      //              rootPath = path.replace(file.name, '');
      //          }
      //      });

      //      //alert(rootFile + "    " + rootPath);

      //      app.view(rootFile, rootPath, fileMap);

      //};

  }



  /**
   * Sets up the view manager.
   * @return {Viewer}
   */
  createViewer () {
    this.viewerEl = document.createElement('div');
    this.viewerEl.classList.add('viewer');
    this.dropEl.innerHTML = '';
    this.dropEl.appendChild(this.viewerEl);
    this.viewer = new Viewer(this.viewerEl, this.options);
    return this.viewer;
  }




  /**
   * Loads a fileset provided by user action.
   * @param  {Map<string, File>} fileMap
   */
  load (fileMap) {
    let rootFile;
    let rootPath;
    Array.from(fileMap).forEach(([path, file]) => {
      if (file.name.match(/\.(gltf|glb)$/)) {
        rootFile = file;
        rootPath = path.replace(file.name, '');
      }
    });

    if (!rootFile) {
      this.onError('No .gltf or .glb asset found.');
    }

    this.view(rootFile, rootPath, fileMap);
  }

  /**
   * Passes a model to the viewer, given file and resources.
   * @param  {File|string} rootFile
   * @param  {string} rootPath
   * @param  {Map<string, File>} fileMap
   */
  view (rootFile, rootPath, fileMap) {

    if (this.viewer) this.viewer.clear();

    const viewer = this.viewer || this.createViewer();

    const fileURL = typeof rootFile === 'string'
      ? rootFile
      : URL.createObjectURL(rootFile);

    const cleanup = () => {
/*      this.hideSpinner();*/
      if (typeof rootFile === 'object') URL.revokeObjectURL(fileURL);
    };

    viewer
      .load(fileURL, rootPath, fileMap)
      .catch((e) => this.onError(e))
      .then((gltf) => {
          if (!this.options.kiosk) {

            //this.validationCtrl.validate(fileURL, rootPath, fileMap, gltf);
        }
        cleanup();
      });
  }

  /**
   * @param  {Error} error
   */
    onError(error) {
        console.log(error);

        let message = (error||{}).message || error.toString();
        if (message.match(/ProgressEvent/)) {
          message = 'Unable to retrieve this file. Check JS console and browser network tab.';
        } else if (message.match(/Unexpected token/)) {
          message = `Unable to parse file content. Verify that this file is valid. Error: "${message}"`;
        } else if (error && error.target && error.target instanceof Image) {
          message = 'Missing texture: ' + error.target.src.split('/').pop();
        }

        window.alert(message);
        console.error(error);
    }

  showSpinner () {
    this.spinnerEl.style.display = '';
  }

  //hideSpinner () {
  //  this.spinnerEl.style.display = 'none';
  //}
}

document.addEventListener('DOMContentLoaded', () => {

    const app = new App(document.body, location);


    var tPanelImg = wijmo.Control.getControl('.TabsArtImagenes');
    tPanelImg.selectedIndexChanged.addHandler(function (s, e) {

        if (s.selectedIndex == 1) {
            //console.log(Imagenes3D);
            //alert("TabsArtImagenes");

            if (Imagenes3D.length > 0) {
                var Imagen3DInit = Imagenes3D[0];
                //var Imagen3DInitFile = RutaImg3D + "/" + Imagen3DInit.Imagen;

                Load_File_3D(Imagen3DInit.Imagen);

                $("#ArtImgViewOthers3D").html("");

                let Cont = 0;
                for (const Img of Imagenes3D) {
                    Cont++;

                    //var ImgAdd = '<div id="ArtImgView3D-' + Cont + '" src="' + '' + '"  alt="" class="FichaArtImgOther3D" />';
                    var ImgAdd = '<div id="ArtImgView3D-' + Cont + '" class="FichaArtImgOther3D " style="text-align: center;">';
                    ImgAdd = ImgAdd + '<i class="far fa-cubes fa-fw" style="font-size: 30px; margin-top: 12px; color: #6c6c6c; "></i>';
                    ImgAdd = ImgAdd + '</div>';

                    $("#ArtImgViewOthers3D").append(ImgAdd);
                    $("#ArtImgView3D-" + Cont).attr("data-fic", Img.Imagen);
                    $("#ArtImgView3D-" + Cont).attr("data-sec", Img.Secuencia);
                }
                $("#ArtImgView3D-1").addClass("ArtImgViewImgSelect3D");

                $(".FichaArtImgOther3D").on("click", function () {
                    var Fichero = $(this).attr('data-fic');
                    Load_File_3D(Fichero);

                    $(".FichaArtImgOther3D").removeClass("ArtImgViewImgSelect3D");
                    $(this).addClass("ArtImgViewImgSelect3D");
                });

            }

        }

    });

    function Load_File_3D(Fichero) {

        //console.log("Load_File_3D: " + Fichero);

        var data = new FormData();
        data.append("Fichero", Fichero);

        $.ajax({
            type: 'POST',
            url: "/Articulos/Get_File_Base64",
            data: data,
            processData: false,
            contentType: false,
            success: function (data) {

                var Fic3D = getFileFromBase64(data, "Fic3D.glb");

                const fileMap = new Map();
                fileMap.set('FicRutaPath', Fic3D);

                let rootFile;
                let rootPath;

                Array.from(fileMap).forEach(([path, file]) => {
                    if (file.name.match(/\.(gltf|glb)$/)) {
                        rootFile = file;
                        rootPath = path.replace(file.name, '');
                    }
                });

                app.view(rootFile, rootPath, fileMap);
            },
            always: function (data) { },
            error: function (xhr, status, error) {
                console.log("Error", xhr, xhr.responseText, status, error);
            }
        });
    }
});
