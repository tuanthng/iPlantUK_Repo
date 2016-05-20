import java.awt.Component;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.Graphics;
import java.awt.Image;
import java.awt.MediaTracker;
import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.MalformedURLException;
import java.net.URL;

import javax.swing.JApplet;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JPanel;
import java.awt.BorderLayout;
import javax.swing.JTextField;
import javax.swing.JTextArea;
import java.awt.Color;
import javax.script.*;


public class RootNavInterface extends JApplet {

	protected static int height = 640;
	protected static int width = 800;

	protected static String STAGING_FOLDER_ROOT = "/home/tuan/staging/";
	
	private Image img;
	private MediaTracker tr;
	private JTextField txtCurrentPath;
	// private MainFrame mainFrame;
	private JTextArea textArea = new JTextArea();
	
	private String[] args;
	
	private String mex;
	private String staging;
	private String stagingParentFolder;
	private String fullStagingFolder;
	private String imgurl ;
	
	private String resultFile;
	
	
	public RootNavInterface() {
		this(null);
		
		
	}

	/**
	 * Create the applet.
	 */
	public RootNavInterface(String args[]) {
		this.args = args;
		
		
	}

//	public void paint(Graphics g) {
//		tr = new MediaTracker(this);
		// just for testing
		
		//img = getImage(getCodeBase(), "0002.jpg.tif");
		//img = getImage(getCodeBase(), "/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
//		try {
			//img = getImage(new URL("file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg"));
			//img = getImage(getDocumentBase(), "file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
		//img = getImage(getCodeBase(), "file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
		//img = getImage(getCodeBase(), "file:///home/tuan/staging/00-4NWciXLTvhKnd2LYVYedVj/0002.jpg");
				
		//	tr.addImage(img, 0);
	//		g.drawImage(img, 0, 0, this);
		//} catch (MalformedURLException e) {
			// TODO Auto-generated catch block
//			e.printStackTrace();
	//	}
	
//	}

	private void initGUI() {
		Object f = getParent();

		while (!(f instanceof Frame)) {
			f = ((Component) f).getParent();
		}

		((Frame) f).setTitle("RootNav Interface");
		
		setSize(new Dimension(width, height));
		
		JMenuBar menuBar = new JMenuBar();
		setJMenuBar(menuBar);

		JMenu mnFile = new JMenu("File");
		menuBar.add(mnFile);

		JMenuItem mntmClose = new JMenuItem("Close");
		mnFile.add(mntmClose);

		JMenu mnView = new JMenu("View");
		menuBar.add(mnView);

		JMenu mnImages = new JMenu("Images");
		mnView.add(mnImages);

		JMenuItem mntmOriginalImage = new JMenuItem("Original Image");
		mnImages.add(mntmOriginalImage);

		JMenuItem mntmProbabilityMap = new JMenuItem("Probability Map");
		mnImages.add(mntmProbabilityMap);

		JMenu mnHelp = new JMenu("Help");
		menuBar.add(mnHelp);

		JMenuItem mntmAbout = new JMenuItem("About");
		mnHelp.add(mntmAbout);		
		
		JPanel pnlMainPanel = new JPanel();
		getContentPane().add(pnlMainPanel, BorderLayout.CENTER);
		
		txtCurrentPath = new JTextField();
		pnlMainPanel.add(txtCurrentPath);
		txtCurrentPath.setColumns(10);
		
		
		textArea.setColumns(10);
		textArea.setRows(20);
		textArea.setBackground(Color.GRAY);
		pnlMainPanel.add(textArea);

	}

	public void init() {
		initGUI();
		
		//readFile();
		if (args != null)
		{
			StringBuffer strBuff = new StringBuffer();
			
			for (String string : args) {
				strBuff.append(string + "\n");
			}
			
			this.txtCurrentPath.setText(strBuff.toString());
		}
		else
		{
			
			
			this.mex = this.getParameter("mex");
			//get image file
			this.imgurl = this.getParameter("image_url");
			this.stagingParentFolder = this.getParameter("stagingFolder");
			
			if (mex != null)
			{
				String[] parts = this.mex.split("/");
				
				this.staging = parts[parts.length - 1]; //assume the staging is the last part of the string
						
				this.fullStagingFolder = this.stagingParentFolder + this.staging;
				
				//this.txtCurrentPath.setText("mex: " + mex + "\n" +  "Staging: " + this.staging);
				
				
				/*if (imgurl != null)
					this.txtCurrentPath.setText("image_url: " + imgurl);
				else
					this.txtCurrentPath.setText("image_url: Dont know." );*/
							
				
				
			}
			/*else 
				this.txtCurrentPath.setText("No parameters");
			
			
			String resouce = this.getParameter("resourcename");
						
			String inputText = this.getParameter("inputText");
			String resourceType = this.getParameter("resourceType");
			String nameInput = this.getParameter("nameInput");
			
			
			this.textArea.setText("Resouce: " + resouce + "\n" + 
			"Input text: " + inputText + 
			"\nResource type: " + resourceType +
			"\nameInput: " + nameInput);*/
			
			this.textArea.setText("URL: " + imgurl + "\n" + 
					"Staging Parent folder: " + stagingParentFolder + 
					"\nFull Staging folder: " + fullStagingFolder );
			/*
			URL location = RootNavInterface.class.getProtectionDomain().getCodeSource().getLocation();
			
			this.txtCurrentPath.setText("location: " + location.getFile());*/
			
			// create a script engine manager
	        /*ScriptEngineManager factory = new ScriptEngineManager();
	        // create a JavaScript engine
	        ScriptEngine engine = factory.getEngineByName("JavaScript");
	        // evaluate JavaScript code from String
	        try {
				engine.eval(" BQFactory.request({ uri: String(myoutputImg)," + 
								"cb: callback(this, 'onResource'), " + 
								"errorcb: callback(this, 'onerror'), " + 
								//uri_params: {view:'short'}, // dima: by default it's short, if error happens we try to mark that in the list by fetched url
								"}); " );
				//get script object
				engine.eval(" var obj = BQFactory.session[" + imgurl + "];" );
				
				Object obj = engine.get("obj");
				
				this.txtCurrentPath.setText(obj.toString());
				 
			} catch (ScriptException e) {
				// TODO Auto-generated catch block
				this.txtCurrentPath.setText(e.getMessage());
			}*/
		}
	}
	
	public void readFile()
	{
		String line;
		// /home/tuan/staging/00-KyhsZw8coxQV7C6s4CnVzj/0002.jpg.tif_result.xml
		//String filename = "/home/tuan/staging/00-KyhsZw8coxQV7C6s4CnVzj/0002.jpg.tif_result.xml";
		String filename = "file:///home/tuan/staging/00-VobuFrwbUHvrAoRLonNc7V/InputData.xml";
		String prHtml = this.getParameter("filename");
		URL url = null;
		
		try{
			url = new URL(filename);
			InputStream in = url.openStream();
			BufferedReader bf = new BufferedReader(new InputStreamReader(in));
			
			StringBuffer strBuff = new StringBuffer();
			
			while((line = bf.readLine()) != null)
			{
				strBuff.append(line + "\n");
			}
			
			this.txtCurrentPath.setText(strBuff.toString());
		}
		catch(Exception e)
		{
			this.txtCurrentPath.setText("Problem: " + e.getMessage());
		}
	}

	public static void main(String[] args) {

		MainFrame frame = new MainFrame(args);
		
		frame.setVisible(true);

	}
}
