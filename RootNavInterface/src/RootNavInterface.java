import java.awt.BorderLayout;
import java.awt.Component;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.Image;
import java.awt.MediaTracker;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileFilter;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.URL;
import java.util.Arrays;
import java.util.Comparator;
import java.util.regex.Pattern;

import javax.swing.BoxLayout;
import javax.swing.JApplet;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JPanel;
import javax.swing.JTextArea;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpression;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;
import org.xml.sax.SAXException;


public class RootNavInterface extends JApplet {
	/**
	 * 
	 */
	private static final long serialVersionUID = -1686644263900653234L;
	protected static int height = 640;
	protected static int width = 800;

	protected static String STAGING_FOLDER_ROOT = "/home/tuan/staging/";

	private Image img;
	private MediaTracker tr;
	
	// private JTextField txtCurrentPath;
	// private MainFrame mainFrame;
	// private JTextArea textArea = new JTextArea();
	
	private JTextArea txtLogArea = new JTextArea();

	private String[] args;

	private String mex;
	private String staging;
	private String stagingParentFolder;
	private String fullStagingFolder;
	private String imgurl;

	private String resultFile;
	
	private String imageFile;
	
	public RootNavInterface() {
		this(null);

	}

	/**
	 * Create the applet.
	 */
	public RootNavInterface(String args[]) {
		this.args = args;

	}

	// public void paint(Graphics g) {
	// tr = new MediaTracker(this);
	// just for testing

	// img = getImage(getCodeBase(), "0002.jpg.tif");
	// img = getImage(getCodeBase(),
	// "/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
	// try {
	// img = getImage(new
	// URL("file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg"));
	// img = getImage(getDocumentBase(),
	// "file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
	// img = getImage(getCodeBase(),
	// "file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
	// img = getImage(getCodeBase(),
	// "file:///home/tuan/staging/00-4NWciXLTvhKnd2LYVYedVj/0002.jpg");

	// tr.addImage(img, 0);
	// g.drawImage(img, 0, 0, this);
	// } catch (MalformedURLException e) {
	// TODO Auto-generated catch block
	// e.printStackTrace();
	// }

	// }

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

		// txtCurrentPath = new JTextField();
		// pnlMainPanel.add(txtCurrentPath);
		// txtCurrentPath.setColumns(10);

		// textArea.setColumns(10);
		// textArea.setRows(20);
		// textArea.setBackground(Color.GRAY);
		// pnlMainPanel.add(textArea);

		JPanel pnlLog = new JPanel();
		getContentPane().add(pnlLog, BorderLayout.SOUTH);
		pnlLog.setLayout(new BoxLayout(pnlLog, BoxLayout.X_AXIS));
		txtLogArea.setEditable(false);

		txtLogArea.setRows(10);
		pnlLog.add(txtLogArea);

	}

	public void init() {
		initGUI();

		// readFile();
		if (args != null) {
			StringBuffer strBuff = new StringBuffer();

			for (String string : args) {
				strBuff.append(string + "\n");
			}

			// this.txtCurrentPath.setText(strBuff.toString());
			writeLog(strBuff.toString(), true);
		} else {

			this.mex = this.getParameter("mex");
			// get image file
			this.imgurl = this.getParameter("image_url");
			this.stagingParentFolder = this.getParameter("stagingFolder");

			if (mex != null) {
				String[] parts = this.mex.split("/");

				this.staging = parts[parts.length - 1]; // assume the staging is
														// the last part of the
														// string

				this.fullStagingFolder = this.stagingParentFolder + this.staging;

				// this.txtCurrentPath.setText("mex: " + mex + "\n" + "Staging:
				// " + this.staging);

				/*
				 * if (imgurl != null) this.txtCurrentPath.setText("image_url: "
				 * + imgurl); else this.txtCurrentPath.setText(
				 * "image_url: Dont know." );
				 */

			}
			/*
			 * else this.txtCurrentPath.setText("No parameters");
			 * 
			 * 
			 * String resouce = this.getParameter("resourcename");
			 * 
			 * String inputText = this.getParameter("inputText"); String
			 * resourceType = this.getParameter("resourceType"); String
			 * nameInput = this.getParameter("nameInput");
			 * 
			 * 
			 * this.textArea.setText("Resouce: " + resouce + "\n" +
			 * "Input text: " + inputText + "\nResource type: " + resourceType +
			 * "\nameInput: " + nameInput);
			 */

			/*
			 * this.textArea.setText("URL: " + imgurl + "\n" +
			 * "Staging Parent folder: " + stagingParentFolder +
			 * "\nFull Staging folder: " + fullStagingFolder );
			 */

			writeLog("URL: " + imgurl + "\n" + "Staging Parent folder: " + stagingParentFolder
					+ "\nFull Staging folder: " + fullStagingFolder, true);

			/*
			 * URL location =
			 * RootNavInterface.class.getProtectionDomain().getCodeSource().
			 * getLocation();
			 * 
			 * this.txtCurrentPath.setText("location: " + location.getFile());
			 */

			// create a script engine manager
			/*
			 * ScriptEngineManager factory = new ScriptEngineManager(); //
			 * create a JavaScript engine ScriptEngine engine =
			 * factory.getEngineByName("JavaScript"); // evaluate JavaScript
			 * code from String try { engine.eval(
			 * " BQFactory.request({ uri: String(myoutputImg)," +
			 * "cb: callback(this, 'onResource'), " +
			 * "errorcb: callback(this, 'onerror'), " + //uri_params:
			 * {view:'short'}, // dima: by default it's short, if error happens
			 * we try to mark that in the list by fetched url "}); " ); //get
			 * script object engine.eval(" var obj = BQFactory.session[" +
			 * imgurl + "];" );
			 * 
			 * Object obj = engine.get("obj");
			 * 
			 * this.txtCurrentPath.setText(obj.toString());
			 * 
			 * } catch (ScriptException e) { // TODO Auto-generated catch block
			 * this.txtCurrentPath.setText(e.getMessage()); }
			 */

			//writeLog("Begin finding xml result files", true);

			String resultPattern = ".*_result.xml";

			//File[] selectedFiles = listFilesMatching(new File("/home/tuan/staging/00-NJqZATtwSAezwX2o53oGyc"),
			//		resultPattern);
			
			boolean test = true;
			
			if (test)
			{
				this.fullStagingFolder = "/home/tuan/staging/00-NJqZATtwSAezwX2o53oGyc";
			}
			
			File[] selectedFiles = listFilesMatching(new File(this.fullStagingFolder), resultPattern);
			// File[] selectedFiles = listFilesMatching(new
			// File(this.fullStagingFolder), resultPattern);

			selectedFiles = sortByLastModifiedDate(selectedFiles);

			if (selectedFiles.length > 0) {
				//this.resultFile = selectedFiles[0].getName();
				this.resultFile = selectedFiles[0].getAbsolutePath();
				
				writeLog("Selected xml result file: " + this.resultFile, true);
				//this.resultFile.concat(this.fullStagingFolder);
				
				// this.txtCurrentPath.setText(selectedFiles[0].getName());
			} else {
				writeLog("No xml result file selected!", true);
				// this.txtCurrentPath.setText("No files found. Check again");
			}

			//writeLog("End finding files", true);
			
			readResultXMLFileOrigin();
		}
	}

	public void readFile() {
		String line;
		// /home/tuan/staging/00-KyhsZw8coxQV7C6s4CnVzj/0002.jpg.tif_result.xml
		// String filename =
		// "/home/tuan/staging/00-KyhsZw8coxQV7C6s4CnVzj/0002.jpg.tif_result.xml";
		String filename = "file:///home/tuan/staging/00-VobuFrwbUHvrAoRLonNc7V/InputData.xml";
		String prHtml = this.getParameter("filename");
		URL url = null;

		try {
			url = new URL(filename);
			InputStream in = url.openStream();
			BufferedReader bf = new BufferedReader(new InputStreamReader(in));

			StringBuffer strBuff = new StringBuffer();

			while ((line = bf.readLine()) != null) {
				strBuff.append(line + "\n");
			}

			// this.txtCurrentPath.setText(strBuff.toString());
			writeLog(strBuff.toString(), true);
		} catch (Exception e) {
			// this.txtCurrentPath.setText("Problem: " + e.getMessage());
			writeLog("Problem: " + e.getMessage(), true);
		}
	}
	/*
	 * //this function uses jdom2. and rewritten by the readResultXMLFileOrigin (without using any external libs)
	 */ 
	/*boolean readResultXMLFile()
	{
		boolean canRead = false;
		
		if (this.resultFile != null)
		{
			File inputFile = new File(this.resultFile);
			
			if (inputFile.exists())
			{
				try {
					SAXBuilder saxBuilder = new SAXBuilder();

				
					Document document = saxBuilder.build(inputFile);
					
					 // use the default implementation
			        XPathFactory xFactory = XPathFactory.instance();
					
			        XPathExpression<Element> expr = xFactory.compile("//DataProcessed/Input/File/ImageFile", Filters.element());
			        List<Element> imgFile = expr.evaluate(document);
			        
			        for(Element el : imgFile)
			        {
			        	writeLog("Image file: " + el.getValue());
			        }
			        
					canRead = true;
				} catch (JDOMException e) {
					// TODO Auto-generated catch block
					canRead = false;
					//e.printStackTrace();
					writeLog(e.getMessage());
				} catch (IOException e) {
					// TODO Auto-generated catch block
					canRead = false;
					//e.printStackTrace();
					writeLog(e.getMessage());
				}
				
			}
		}
		
		return canRead;
	}*/
	
	boolean readResultXMLFileOrigin()
	{
		boolean canRead = false;
		
		if (this.resultFile != null)
		{
			File inputFile = new File(this.resultFile);
			
			if (inputFile.exists())
			{
				try {
					DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();

					DocumentBuilder builder = factory.newDocumentBuilder();
					
					Document doc = builder.parse(inputFile);
					
					 XPathFactory xpathFactory = XPathFactory.newInstance();
					 XPath xpath = xpathFactory.newXPath();
					 XPathExpression expr = xpath.compile("//DataProcessed/Input/File/ImageFile");
					 
					 NodeList nl = (NodeList)expr.evaluate(doc, XPathConstants.NODESET);
					 
					 for(int index = 0; index < nl.getLength(); index++)
					 {
						 Node n = nl.item(index);
						 
						 if (n.getNodeType() == Node.ELEMENT_NODE)
						 {
							 org.w3c.dom.Element el = (Element)n;
							 this.imageFile = el.getTextContent();
							 
							 writeLog("Image file: " + this.imageFile);
						 }
						 
					 }
			        
					canRead = true;
				}catch (ParserConfigurationException e) {
					//e.printStackTrace();
					writeLog(e.getMessage());
				} catch (SAXException e) {
					//e.printStackTrace();
					writeLog(e.getMessage());
				} catch (IOException e) {
					//e.printStackTrace();
					writeLog(e.getMessage());
				} catch (XPathExpressionException e) {
					//e.printStackTrace();
					writeLog(e.getMessage());
				}
				
			}
		}
		
		return canRead;
	}
	private void writeLog(String text, boolean newLine) {
		this.txtLogArea.append(text);
		if (newLine)
			this.txtLogArea.append("\n");

	}
	private void writeLog(String text) {
		writeLog(text, true);

	}

	public static File[] listFilesMatching(File root, String regex) {
		if (!root.isDirectory()) {
			throw new IllegalArgumentException(root + " is no directory.");
		}
		final Pattern p = Pattern.compile(regex); // careful: could also throw
													// an exception!
		// return root.listFiles();
		return root.listFiles(new FileFilter() {
			@Override
			public boolean accept(File file) {
				return p.matcher(file.getName()).matches();
				//return p.matcher(file.getAbsolutePath()).matches();
			}
		});
	}

	public static File[] sortByLastModifiedDate(File[] files) {
		Arrays.sort(files, new Comparator<Object>() {
			public int compare(Object o1, Object o2) {

				if (((File) o1).lastModified() > ((File) o2).lastModified()) {
					return -1;
				} else if (((File) o1).lastModified() < ((File) o2).lastModified()) {
					return +1;
				} else {
					return 0;
				}
			}

		});

		return files;
	}

	public static void main(String[] args) {

		MainFrame frame = new MainFrame(args);

		frame.setVisible(true);

	}
}
