
// overwrite standard renderer with our own
BQ.renderers.resources.tag = 'BQ.javaappletex.Tag';
BQ.renderers.resources.mex = 'BQ.javaappletex.Mex';

var rsmlcontent = "";
var rsmlfile = "";
var fileContent = "";

var htmlWidget = function( html, title ) {
    var w = Ext.create('Ext.window.Window', {
        modal: true,
        //width: BQApp?BQApp.getCenterComponent().getWidth()/1.6:document.width/1.6,
        //height: BQApp?BQApp.getCenterComponent().getHeight()/1.2:document.height/1.2,
		width: 1230,
        height: 820,
        buttonAlign: 'center',
        autoScroll: true,
        //html: html, // set html after the layout was done for proper sizing with 100% width and height 
        buttons: [ { text: 'Close', handler: function () { w.close(); } }],
        title: (title && typeof title == 'string') ? title : undefined,
    });
    w.show(); 
    w.update(html); // set html after the layout was done for proper sizing with 100% width and height 
}; 

Ext.define('BQ.javaappletex.AppletRunner', {
    runApplet: function() {
        var mex = this.mex;
        var inputs = mex.inputs;
        
        var stagingFolder = '/home/tuan/staging/';
        
        var params = '';
        var par = new Ext.Template('<param name="{name}" value="{value}">');
        var ii = undefined;
        var imgurl = '';
        
        var nameInput = 'InputName: ';
        for (var i=0; ii=inputs[i]; ++i) {
            if (ii.type !== 'system-input')
            	{
                	params += par.apply({name: ii.name, value: ii.value});
                	if (ii.name == 'image_url')
                		{
                			imgurl = ii.value;
                		}
            	}
            /*else if(ii.type == 'system-input')
            	{
            		
            	}*/
            
            nameInput = nameInput.concat(String(ii.name), "  ");
            
        }
        
        //this.image_names = {};
        //var resource = this.resources;
        var myoutputImg = mex.dict['outputs/OutputImage']; //this works
        var numbertipsdetected = mex.dict['outputs/Tip(s) detected'];
        
        rsmlfile = mex.dict['outputs/rsml'];
        //this.images = Ext.clone(mex.iterables[myiterable]);
        var textKey = "";
        var textValue = "";
        
        this.imageName = "Here is an image";
        
        for(var key in mex.dict)
        	{
        	textKey = textKey.concat(String(key), "\n");
        	}
        
        var resource = this.resource;
        var template = resource.template;
        var resourceType = resource.resource_type;
        
        /*var inputs = this.ms.module.inputs;
        var inputText = "No thing";
        
        if (inputs != null && inputs.length> 0)
        	{
        	for (var p=0; (i=inputs[p]); p++) {
                var t = i.type;
                if (t in BQ.selectors.resources && t == 'image')
                	{
                	inputText = "An image";
                	}
                    
                                                       
        	}
        	}*/
        
        
      // BQFactory.request({ uri: this.images['dataset'], 
      //      cb: callback(this, 'onResource'), 
       //     errorcb: callback(this, 'onerror'), 
            //uri_params: {view:'short'}, 
       //  });       
        
        /*BQFactory.request({ uri: String(myoutputImg), 
            cb: callback(this, 'onResource'), 
            errorcb: callback(this, 'onerror'), 
            //uri_params: {view:'short'}, // dima: by default it's short, if error happens we try to mark that in the list by fetched url
        }); 
        
        BQFactory.load(String(myoutputImg), 'onResource'));
        
        var o = BQFactory.session[String(myoutputImg)];
        
        if (o == null)
        	{
        	resourceType = "No image in resource type.";
        	}
        else
        	{
        	if (o instanceof BQImage)     

        		resourceType = "Resouce type BQImage: ";
        	else if (o instanceof XMLHttpRequest)
        		{
        			resourceType =  ((XMLHttpRequest)o).responseXML;
        		}
        	else 
        		{ 
        			resourceType = o.toString();
        		}
        		
        	}*/
        
        //BQFactory.load (String(myoutputImg), callback(this, 'onResource'));
        
        /*var html = new Ext.Template('<object type="application/x-java-applet" height="100%" width="100%" >\
            <param name="code" value="RootNavInterface" />\
            <param name="archive" value="RootNavInterface.jar" />\
            <param name="java_arguments" value="-Djnlp.packEnabled=true"/>\
            <param name="scriptable" value="true" />\
            <param name="mayscript" value="true" />\
        	<param name="stagingFolder" value="' + stagingFolder + '" />\
        	<param name="image_url" value="' + String(myoutputImg) + '">\
        	<param name="resourcename" value="' + String(resource.name) + '">\
        	<param name="resourceType" value="' + String(resourceType) + '">\
        	<param name="nameInput" value="' + String(nameInput) + '">\
        	<param name="mex" value="{url}">\
        	{params}\
            Applet failed to run.  No Java plug-in was found.\
        </object>');*/
        var html = new Ext.Template('<object type="application/x-java-applet" height="100%" width="100%" >\
                <param name="code" value="RootNavInterface" />\
                <param name="archive" value="RootNavInterface.jar" />\
                <param name="java_arguments" value="-Djnlp.packEnabled=true"/>\
                <param name="scriptable" value="true" />\
                <param name="mayscript" value="true" />\
            	<param name="stagingFolder" value="' + stagingFolder + '" />\
            	<param name="image_url" value="' + String(myoutputImg) + '">\
            	<param name="mex" value="{url}">\
            	{params}\
                Applet failed to run.  No Java plug-in was found.\
            </object>');
        htmlWidget(html.apply({url: mex.uri, params: params}), 'RootNav');
    },
    
    onerror: function (e) {
        BQ.ui.error(e.message);  
        this.imageName = "No images found.";
        //this.num_requests--;
        //if (e.request.request_url in this.images)
        //    delete this.images[e.request.request_url];
        //if (this.num_requests<=0) this.onAllImages();
    }, 
    
    onResource: function(im) {
        //this.num_requests--;
    	//window.alert(String(im.name));
    	this.imageName = im.name;
        //print (String(im));
    	if (im instanceof BQImage)     
            //this.images[im.uri].name = im.name;
        	this.imageName = im.name;
        else if (im instanceof BQDataset)
            this.dataset_name = im.name;                 
        //if (this.num_requests<=0) this.onAllImages();
    },
});

// provide our renderer
Ext.define('BQ.javaappletex.Tag', {
    extend: 'BQ.renderers.Tag',
    mixins: {
        canRun: 'BQ.javaappletex.AppletRunner',
    },
    
    initComponent : function() {
        this.callParent();
        
        this.insert(0, {
            xtype:   'button',
            text:    'Run extended analysis', 
            iconCls: 'applet', 
            scale:   'large', 
            handler: Ext.Function.bind( this.runApplet, this ),
        });           
    },
});

Ext.define('BQ.javaappletex.Mex', {
    extend: 'BQ.renderers.Mex',
    mixins: {
        canRun: 'BQ.javaappletex.AppletRunner',
    },
    
    initComponent : function() {
        this.callParent();
        this.insert(0, {
            xtype:   'button',
            text:    'Run extended analysis', 
            iconCls: 'applet', 
            scale:   'large', 
            handler: Ext.Function.bind( this.runApplet, this ),
        });           
    },
});

function loadScript(url, callback)
{
    // Adding the script tag to the head as suggested before
    var head = document.getElementsByTagName('head')[0];
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    // Then bind the event to the callback function.
    // There are several events for cross browser compatibility.
    script.onreadystatechange = callback;
    script.onload = callback;

    // Fire the loading
    head.appendChild(script);
}

function downloadCode(url, callback)
{
	//window.alert('I\'m here, waiting');
}

/**
 * read text input
 */
function readText(filePath) {
    var output = ""; //placeholder for text output
    //if(filePath.files && filePath.files[0]) {
    var reader = new FileReader();
        reader.onload = function (e) {
            output = e.target.result;
            
            //return output;
        };//end onload()
        //reader.readAsText(filePath.files[0]);
        reader.readAsText(new File(filePath));
        download("myrsml.rsml", output);
        //return output;
    //}//end if html5 filelist support
   /* else if(ActiveXObject && filePath) { //fallback to IE 6-8 support via ActiveX
        try {
            reader = new ActiveXObject("Scripting.FileSystemObject");
            var file = reader.OpenTextFile(filePath, 1); //ActiveX File Object
            output = file.ReadAll(); //text contents of file
            file.Close(); //close file "input stream"
            
            return output;
            
        } catch (e) {
            if (e.number == -2146827859) {
                alert('Unable to access local files due to browser security settings. ' + 
                 'To overcome this, go to Tools->Internet Options->Security->Custom Level. ' + 
                 'Find the setting for "Initialize and script ActiveX controls not marked as safe" and change it to "Enable" or "Prompt"'); 
            }
        }       var textread = "";
    }
    else { //this is where you could fallback to Java Applet, Flash or similar
        return false;
    }       
    */
    
    return true;
}   



function readTextFile(file)
{
	var allText = "";
    var rawFile = new XMLHttpRequest();
    rawFile.open("GET", file, true);
    rawFile.onreadystatechange = function ()
    {
        if(rawFile.readyState === 4)
        {
            if(rawFile.status === 200 || rawFile.status == 0)
            {
                allText = rawFile.responseText;
                fileContent = allText;
                //alert(allText);
            }
        }
    }
    rawFile.send(null);
    
    return allText;
}

function download(filename, text) 
{
	var element = document.createElement('a');
	element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
	element.setAttribute('download', filename);
	
	element.style.display = 'none';
	document.body.appendChild(element);
	
	element.click();
	
	document.body.removeChild(element);
}
function downloadURI(filename) 
{
	var element = document.createElement('a');
	
	element.style.display = 'none';
	element.href = filename;
	//document.body.appendChild(element);
	
	element.click();
	
	//document.body.removeChild(element);
}

//overwrite standard renderer with our own
Ext.onReady( function() {
    BQ.renderers.resources.image = 'BQ.renderers.rootnav.Image';
});

// provide our renderer
Ext.define('BQ.renderers.rootnav.Image', {
    extend: 'BQ.renderers.Image',

    afterRender : function() {
        this.callParent();
        this.queryById('bar_bottom').add({
            xtype: 'tbspacer',
            width: 15,
        }, {
            xtype: 'colorfield',
            itemId: 'color1',
            cls: 'simplepicker',
            labelWidth: 0,
            name: 'color1',
            value: '0000ff',
            listeners: {
                scope: this,
                change: this.onNewGradient,
            },
        }, {
            xtype: 'colorfield',
            itemId: 'color2',
            cls: 'simplepicker',
            labelWidth: 0,
            name: 'color2',
            value: 'ffff00',
            listeners: {
                scope: this,
                change: this.onNewGradient,
            },
        }, {
            xtype: 'tbspacer',
            width: 15,
        }, {
            xtype: 'slider',
            width: 200,
            value: 0,
            animate: false,
            increment: 1,
            minValue: 0,
            maxValue: 100,
            listeners: {
                scope: this,
                change: this.onFilter,
            },
        }, {
            xtype: 'tbspacer',
            width: 20,
        }, {
            xtype:'button',
            text: 'Download RSML',
            tooltip: 'Download RSML to your local location',
            scope: this,
            handler: this.save,
        }, {
            xtype: 'tbspacer',
            flex: 1,
        });
        //var numbertipsdetected = mex.dict['outputs/Tip(s) detected'];
        //loadScript("download.js", downloadCode);
    },

    onFilter : function(slider, newvalue) {
        BQGObject.confidence_cutoff = newvalue;
        var me = this;
        clearTimeout(this.updatetimer);
        this.updatetimer = setTimeout(function(){ me.reRenderGobs(); }, 50);
    },

    doFilter : function() {
        // request re-rendering of gobjects
        this.reRenderGobs();
    },

    save : function() {
    	
    	//this.setLoading('Generating...');
    	
    	//readText("file:///home/tuan/staging/00-nUvqT6qX9kMTgENP4rRti3/P.rsml");
    	//download("myrsml.rsml", readText("file:///home/tuan/staging/00-nUvqT6qX9kMTgENP4rRti3/P.rsml"));
    	//downloadTextFile("file:///home/tuan/staging/00-nUvqT6qX9kMTgENP4rRti3/P.rsml");
    	//readTextFile("file:///home/tuan/staging/00-nUvqT6qX9kMTgENP4rRti3/P.rsml");
    	
    	//readTextFile("file://admin/2016-09-06/P.rsml");
    	
    	
    	//alert('numbertipsdetected:' + numbertipsdetected);
    	var rsmlfile = this.mex.dict['outputs/RSMLFile'];
    	var rsmlname = this.mex.dict['outputs/RSMLName'];
    	//alert('filersml:' + rsmlfile);
    	//var textKey = "";

    	//for(var key in this.mex.dict)
        //{
        //	textKey = textKey.concat(String(key), "\n");
        //}
    	//alert('Value: ' + this.mex.dict['outputs/OutputRootsImage']);
    	
    	//There are 2 ways to do the download file
    	//1. call readTextFile to fill fileContent variable. Then, call 
    	//readTextFile("http://127.0.0.1:8080/blob_service/00-NUkzLMk54qqUfATRVucXDR");
    	//download("myrsml.rsml", fileContent);
    	//readTextFile(rsmlfile);
    	//download(rsmlname, fileContent);
    	//2. call downloadURI
    	//downloadURI("http://127.0.0.1:8080/blob_service/00-NUkzLMk54qqUfATRVucXDR");
    	downloadURI(rsmlfile);
    	
    	//Note: if use the 1. method, need to find the filename. The 2 method doesnt need that.
    	
        /*var me = this,
            points = this.gobjects[0];
        this.setLoading('Generating...');
        // filter gobjects first
        points.gobjects = points.gobjects.filter(function(g){
            try {
                var confidence = g.gobjects[0].tags[0].value;
                if (confidence<BQGObject.confidence_cutoff) return false;
            } catch (e) {
                return true;
            }
            return true;
        });
		*/
        //this.setLoading('Downloading...');
    	this.setLoading(false);
        /*points.save_(
            undefined,
            function() {
                me.setLoading(false);
            },
            function() {
                me.setLoading(false);
                BQ.ui.error('Problem saving filtered points');
            }
        );*/
    },

    onNewGradient: function() {
        var c1 = this.queryById('color1').getColor(),
            c2 = this.queryById('color2').getColor();
        bq_create_gradient(c1.r,c1.g,c1.b,1.0, c2.r,c2.g,c2.b,1.0);
        this.reRenderGobs();
    }

});


