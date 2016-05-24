import java.awt.Dimension;
import java.awt.Insets;
import java.awt.Toolkit;

import javax.swing.JFrame;

public class MainFrame extends JFrame {
	
	private String[] args;
	
	public MainFrame()
	{
		initGUI();
	}
	

	public MainFrame(String[] args)
	{
		this.args = args;
		initGUI();
	}
	
	private void initGUI()
	{
		// create the frame with a title
		this.setTitle("RootNav Interface");
		// exit the application when the JFrame is closed
		this.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		
		//setLayout(new FlowLayout()); 
		
		// pack the frame to get correct insets
		this.pack();
		
		Insets fI = this.getInsets();
		this.setSize(RootNavInterface.width + fI.right + fI.left, RootNavInterface.height + fI.top + fI.bottom);
		// center the frame on screen
		Dimension sD = Toolkit.getDefaultToolkit().getScreenSize();
		this.setLocation((sD.width - RootNavInterface.width) / 2, (sD.height - RootNavInterface.height) / 2);
		// make the frame visible

		// add the applet to the frame
		this.getContentPane().add(new RootNavInterface(args));
	
	}

}
