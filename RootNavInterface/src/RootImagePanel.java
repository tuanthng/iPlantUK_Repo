import java.awt.Dimension;
import java.awt.Graphics;
import java.awt.Image;
import java.awt.MediaTracker;
import java.awt.image.BufferedImage;
import java.awt.image.RenderedImage;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.channels.FileChannel;

import javax.imageio.ImageIO;
import javax.media.jai.PlanarImage;
import javax.swing.JPanel;

import com.sun.media.jai.codec.ByteArraySeekableStream;
import com.sun.media.jai.codec.ImageCodec;
import com.sun.media.jai.codec.ImageDecoder;
import com.sun.media.jai.codec.SeekableStream;

public class RootImagePanel extends JPanel 
{
	/**
	 * 
	 */
	private static final long serialVersionUID = 3426680246261912876L;

	public enum IMG_DISPLAY
	{
		ORIGIN,
		PROBABILITY
	}
	
	private char pathSeparator = '/';
	private char extensionSeparator = '.';
	    
	private BufferedImage image;
	private MediaTracker tr;
	private Image originalImg;

	private Image probabilityImg;
	
	private IMG_DISPLAY imgChoice = IMG_DISPLAY.PROBABILITY;
	
	public void setOriginalImage(Image img) 
	{
		this.originalImg = img;
		this.repaint();
	}

	public void setOriginalImage(String filename) 
	{
		try 
		{

			int dot = filename.lastIndexOf(extensionSeparator);
			String ext = filename.substring(dot + 1);

			if (ext.equals("tif")) 
			{
				this.originalImg = loadTiffFile(filename);
			} 
			else 
			{
				this.originalImg = ImageIO.read(new File(filename));
			}

			this.repaint();

		}
		catch (IOException e) 
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	public void setProbabilityImage(Image img) 
	{
		this.probabilityImg = img;
		this.repaint();
	}
	
	public void setProbabilityImage(String filename) 
	{
		try 
		{

			int dot = filename.lastIndexOf(extensionSeparator);
			String ext = filename.substring(dot + 1);

			if (ext.equals("tif")) 
			{
				this.probabilityImg = loadTiffFile(filename);
			}
			else 
			{
				this.probabilityImg = ImageIO.read(new File(filename));
			}

			this.repaint();

		} 
		catch (IOException e) 
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	/*
	 * protected void paintComponent (Graphics g) { super.paintComponent(g);
	 * 
	 * tr = new MediaTracker(this); // just for testing
	 * 
	 * tr.addImage(this.originalImg, 0); g.drawImage(this.originalImg, 0, 0,
	 * this);
	 * 
	 * }
	 */
	public void paint(Graphics g) {
		super.paintComponent(g);

		tr = new MediaTracker(this);
		// just for testing
		
		if (imgChoice == IMG_DISPLAY.ORIGIN)
		{
			tr.addImage(this.originalImg, 0);
			g.drawImage(this.originalImg, 0, 0, this);
		}
		else if (imgChoice == IMG_DISPLAY.PROBABILITY)
		{
			tr.addImage(this.probabilityImg, 0);
			g.drawImage(this.probabilityImg, 0, 0, this);
		}
	}
	
	public Dimension getPreferredSize() 
	{
		if (imgChoice == IMG_DISPLAY.PROBABILITY)
		{
			return probabilityImg == null ? new Dimension(400, 400) : new Dimension(probabilityImg.getWidth(null), probabilityImg.getHeight(null));
		}
		else if (imgChoice == IMG_DISPLAY.ORIGIN)
		{
			return originalImg == null ? new Dimension(400, 400) : new Dimension(originalImg.getWidth(null), originalImg.getHeight(null));
		}
        return new Dimension(400, 400) ;
    }
	
	static Image load(byte[] data) 
	{
		Image image = null;
		SeekableStream stream;
		try {
			stream = new ByteArraySeekableStream(data);
			String[] names = ImageCodec.getDecoderNames(stream);
			ImageDecoder dec = ImageCodec.createImageDecoder(names[0], stream, null);
			RenderedImage im = dec.decodeAsRenderedImage();
			image = PlanarImage.wrapRenderedImage(im).getAsBufferedImage();
			
		} 
		catch (IOException e) 
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		return image;
	}

	public static Image loadTiffFile(String path) 
	{
		FileInputStream in;
		Image image = null;
		
		try 
		{
			in = new FileInputStream(path);
			FileChannel channel = in.getChannel();
			ByteBuffer buffer = ByteBuffer.allocate((int) channel.size());
			channel.read(buffer);
			image = load(buffer.array());
		}
		catch (FileNotFoundException e) 
		{
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		return image;

	}

}
