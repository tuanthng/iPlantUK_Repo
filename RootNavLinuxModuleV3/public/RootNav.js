
// overwrite standard renderer with our own
BQ.renderers.resources.tag = 'BQ.javaappletex.Tag';
BQ.renderers.resources.mex = 'BQ.javaappletex.Mex';

var htmlWidget = function( html, title ) {
    var w = Ext.create('Ext.window.Window', {
        modal: true,
        width: BQApp?BQApp.getCenterComponent().getWidth()/1.6:document.width/1.6,
        height: BQApp?BQApp.getCenterComponent().getHeight()/1.2:document.height/1.2,
        buttonAlign: 'center',
        autoScroll: true,
        //html: html, // set html after the layout was done for proper sizing with 100% width and height 
        buttons: [ { text: 'Ok', handler: function () { w.close(); } }],
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
        
        var myinputImg = mex.dict['inputs'];
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


