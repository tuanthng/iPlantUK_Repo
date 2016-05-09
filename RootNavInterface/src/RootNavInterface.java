import java.awt.Component;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.Graphics;
import java.awt.Image;
import java.awt.MediaTracker;
import java.net.MalformedURLException;
import java.net.URL;

import javax.swing.JApplet;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;

public class RootNavInterface extends JApplet {

	protected static int height = 640;
	protected static int width = 800;

	private Image img;
	private MediaTracker tr;
	// private MainFrame mainFrame;

	public RootNavInterface() {
		this(null);

	}

	/**
	 * Create the applet.
	 */
	public RootNavInterface(String args[]) {
		
	}

	public void paint(Graphics g) {
		tr = new MediaTracker(this);
		// just for testing
		
		//img = getImage(getCodeBase(), "0002.jpg.tif");
		//img = getImage(getCodeBase(), "/home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
//		try {
			//img = getImage(new URL("file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg"));
			//img = getImage(getDocumentBase(), "file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
		img = getImage(getCodeBase(), "file:///home/tuan/bisque/modules/RootNavLinuxModuleV3/0002.jpg");
			
			tr.addImage(img, 0);
			g.drawImage(img, 0, 0, this);
		//} catch (MalformedURLException e) {
			// TODO Auto-generated catch block
//			e.printStackTrace();
	//	}
		
	}

	private void initGUI() {
		Object f = getParent();

		while (!(f instanceof Frame)) {
			f = ((Component) f).getParent();
		}

		((Frame) f).setTitle("RootNav Interface 111");
		
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

	}

	public void init() {
		initGUI();
	}

	public static void main(String[] args) {

		MainFrame frame = new MainFrame();
		
		frame.setVisible(true);

	}
}
