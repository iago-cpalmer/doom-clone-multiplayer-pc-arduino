package com.iago.doomlvleditor.view;

import java.awt.BasicStroke;
import java.awt.Color;
import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.awt.image.BufferedImage;
import java.awt.image.IndexColorModel;
import java.util.Random;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import com.iago.doomlvleditor.model.Line;
import com.iago.doomlvleditor.model.Model;
import com.iago.doomlvleditor.model.Vector2;

public class BoardPanel extends JPanel implements MouseListener{

	
	private Model model;
	
	private int xi, xf, yi,yf;
	private Vector2[] selected;
	private int pointToChange = 0;
	private Color clrHead = new Color(0,100,0);
	private Color clrTail = Color.GREEN;
	private final int DOT_PX_SIZE = 2;
	private BufferedImage obstacleBuffer = null;
	
	public BoardPanel(Model model) {
		this.setFocusable(true); // Make the panel focusable
	    this.requestFocusInWindow(); // Request focus
		this.model = model;
		this.addMouseListener(this);
		selected = new Vector2[2];
		selected[0] = new Vector2(-1,-1);
		selected[1] = new Vector2(-1,-1);
	}
	
	
	@Override
	public void paint(Graphics g) {
		Graphics2D g2d = (Graphics2D) g;
		// Board
		int cols = model.getCols();
		int rows = model.getRows();
		
		int size = 0;
		if(this.getWidth()>this.getHeight()) {
			size = this.getHeight();
		} else {
			size = this.getWidth();
		}
		
		if(obstacleBuffer==null/* || obstacleBuffer.getWidth()!=this.getWidth()|| obstacleBuffer.getHeight()!=this.getHeight()*/) {
			obstacleBuffer = new BufferedImage(this.getWidth(), this.getHeight(),BufferedImage.TYPE_INT_ARGB);
			
		}
		obstacleBuffer.getGraphics().setColor(Color.BLUE);
		obstacleBuffer.getGraphics().clearRect(0, 0, this.getWidth(), this.getHeight());
		int offset = 0;
		if(rows > cols) {
			offset = Math.floorDiv(size, rows);
		} else {
			offset = Math.floorDiv(size, cols);
		}
		
		
		g2d.setColor(Color.BLACK);
		g2d.setStroke(new BasicStroke(3));
		
		xi=(this.getWidth()-offset*cols)/2 + offset/2;
		yi = (this.getHeight() - offset*rows)/2 + offset/2;
		
		yf = offset*rows + yi;
		xf = offset*cols + xi;
		
		for(int x = 0; x < cols; x++) {
			for(int y = 0; y < rows; y++) {
				paintDot(x*offset + xi, y*offset+yi, DOT_PX_SIZE, g2d);
			}
		}
		
		// Draw all lines
		int c = 0;
		// Obtain the Graphics2D object from obstacleBuffer once
		Graphics2D obstacleGraphics = (Graphics2D) obstacleBuffer.getGraphics();

		// Set the stroke for obstacleGraphics
		obstacleGraphics.setStroke(new BasicStroke(20));

		for(Line l : model.getLines()) {
		    // Draw the line with g2d context
			g2d.setColor(model.obstacleColors[l.getType()]);
		   
		    g2d.drawLine(l.getPointA().getX()*offset+xi, l.getPointA().getY()*offset+yi, l.getPointB().getX()*offset+xi, l.getPointB().getY()*offset+yi);

		    // Set the color for the obstacleGraphics context
		    obstacleGraphics.setColor(new Color(c+1));

		    // Draw the line with obstacleGraphics context
		    obstacleGraphics.drawLine(l.getPointA().getX()*offset+xi, l.getPointA().getY()*offset+yi, l.getPointB().getX()*offset+xi, l.getPointB().getY()*offset+yi);

		    c++;
		}

		// Dispose of the graphics context when done
		obstacleGraphics.dispose();

		//g2d.drawImage(obstacleBuffer, 0,0, obstacleBuffer.getWidth(), obstacleBuffer.getHeight(), null);
		// Draw selection
		for(Vector2 s:selected) {
			if(s.getX()==-1) {
				return;
			}
			if(s==selected[pointToChange]) {
				g2d.setColor(Color.red);
			} else {
				g2d.setColor(Color.blue);
			}
			int xp = s.getX()*offset;
			int yp = s.getY()*offset;
			paintDot(xp+xi, yp+yi, DOT_PX_SIZE*4, g2d);
		}
		g2d.setColor(Color.cyan);
		g2d.drawLine(selected[0].getX()*offset+xi, selected[0].getY()*offset+yi, selected[1].getX()*offset+xi, selected[1].getY()*offset+yi);
		
		
		
		
	}

	public void paintDot(int x, int y, int dotSize, Graphics2D g2d) {
		g2d.fillOval(x-dotSize, y-dotSize, dotSize*2, dotSize*2);
	}
	
	public void paintArrow(int xi, int xf, int yi, int yf, Graphics2D g2d) {
		g2d.drawLine(xi, yi, xf, yf);
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		// TODO Auto-generated method stub
		
	}


	@Override
	public void mousePressed(MouseEvent e) {
		// TODO Auto-generated method stub
		
	}


	@Override
	public void mouseReleased(MouseEvent e) {
		int xRelCoord = e.getX();
		int yRelCoord = e.getY();
		int xBoard;
		int yBoard;
		if(!insideBoard(xRelCoord, yRelCoord)) {
			return;
		}
		int offset = ((xf-xi)/model.getCols());
		xBoard = (xRelCoord-xi+offset/2)/offset;
		yBoard = (yRelCoord-yi + offset/2)/offset;
		Vector2 a = new Vector2(xRelCoord, yRelCoord);
		Vector2 b = new Vector2(xBoard*offset + xi, yBoard*offset + yi);
		switch(e.getButton()) {
			case MouseEvent.BUTTON1:
				// select point
				
				//selected[1] = selected[0];
				selected[pointToChange] = new Vector2(xBoard, yBoard);
				pointToChange = (pointToChange+1)%2;
				break;
			case MouseEvent.BUTTON2:
				pointToChange = (pointToChange+1)%2;
				break;
			case MouseEvent.BUTTON3:
				
				// delete wall on mouse position
				int lineId = obstacleBuffer.getRGB(xRelCoord, yRelCoord) + 16777216;
				if(lineId>0) {
					lineId--;
					model.getLines().remove(lineId);
				}
				
				break;
		}
	}


	@Override
	public void mouseEntered(MouseEvent e) {
		// TODO Auto-generated method stub
		
	}


	@Override
	public void mouseExited(MouseEvent e) {
		// TODO Auto-generated method stub
		
	}
	
	public boolean insideBoard(int x, int y) {
		return x >=xi && x<xf && y>=yi && y<yf;
	}
	
	public void create(int type) {
		model.getLines().add(new Line(selected[0].clone(), selected[1].clone(), type));
	}

}
