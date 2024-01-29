package com.iago.doomlvleditor.view;

import java.awt.BorderLayout;
import java.awt.Desktop;
import java.awt.Dimension;
import java.awt.FlowLayout;
import java.awt.HeadlessException;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.KeyEvent;
import java.awt.event.KeyListener;
import java.beans.PropertyChangeListener;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import javax.swing.Action;
import javax.swing.ActionMap;
import javax.swing.InputMap;
import javax.swing.JButton;
import javax.swing.JComponent;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JSeparator;
import javax.swing.JTextField;
import javax.swing.KeyStroke;
import javax.swing.SwingConstants;

import org.json.JSONArray;
import org.json.XMLTokener;

import com.iago.doomlvleditor.model.Line;
import com.iago.doomlvleditor.model.Model;

public class View extends JFrame implements ActionListener{
	private Model model;
	
	private BoardPanel board;
	private JButton btnSave;
	private JButton btnSaveCpp;
	private JButton[] obstacleButtons;
	private JButton btnLoad;
	private JButton btnShow;
	private JButton btnReset;
	private JTextField tfCols;
	private JTextField tfRows;
	private JTextField tfSteps;
	
	private final int MIN_WIDTH = 1000, MIN_HEIGHT = 1000;
	
	public View(Model model){
		this.model = model;
		obstacleButtons = new JButton[model.types.length];
		this.setTitle("Doom Online Clone Level Editor");
        this.setMinimumSize(new Dimension(MIN_WIDTH, MIN_HEIGHT));
        this.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        this.setLayout(new BorderLayout());
        this.setLocationRelativeTo(null);
        this.getContentPane().setLayout(new BorderLayout());
        
        this.board = new BoardPanel(model);
        this.add(board, BorderLayout.CENTER);
        
        btnSave = new JButton("Save");
        btnSave.addActionListener(this);
        btnSaveCpp = new JButton("Save Cpp");
        btnSaveCpp.addActionListener(this);
        btnLoad = new JButton("Load");
        btnLoad.addActionListener(this);
        btnShow = new JButton("Show in explorer");
        btnShow.addActionListener(this);
        btnReset = new JButton("Reset");
        btnReset.addActionListener(this);
        
        int i = 0;
        for(String s:model.types) {
        	obstacleButtons[i] = new JButton(s);
        	obstacleButtons[i++].addActionListener(this);
        }
        
        tfCols = new JTextField(model.getCols()+"");
        tfCols.setPreferredSize(new Dimension(50, tfCols.getPreferredSize().height)); // Set preferred width
        tfCols.addActionListener(this);
        tfRows = new JTextField(model.getRows()+"");
        tfRows.addActionListener(this);
        tfRows.setPreferredSize(new Dimension(50, tfRows.getPreferredSize().height)); // Set preferred width
        
        
        JSeparator verticalSeparator = new JSeparator(SwingConstants.VERTICAL);
        Dimension size = new Dimension(2, 30); // Set the width and height of the separator
        verticalSeparator.setPreferredSize(size);
        
        JSeparator verticalSeparator2 = new JSeparator(SwingConstants.VERTICAL);
        verticalSeparator2.setPreferredSize(size);
        
        JPanel buttons = new JPanel(new FlowLayout());
        buttons.add(btnSave);
        buttons.add(btnSaveCpp);
        buttons.add(btnLoad);
        buttons.add(btnShow);
        buttons.add(btnReset);
        buttons.add(verticalSeparator);
        
        for(int h = 0; h < obstacleButtons.length; h++) {
        	buttons.add(obstacleButtons[h]);
        }
        buttons.add(verticalSeparator2);
        buttons.add(new JLabel("Columns: "));
        buttons.add(tfCols);
        buttons.add(new JLabel("Rows: "));
        buttons.add(tfRows);
        this.add(buttons, BorderLayout.NORTH);
        
        this.pack();
        this.setVisible(true);
	}

	@Override
	public void actionPerformed(ActionEvent e) {
		int obstacleButton;
		if(e.getSource()==btnSave) {
			// Save to file
			
			JFileChooser fileChooser = new JFileChooser();
			fileChooser.setCurrentDirectory(new File("C:\\Users\\joanc\\OneDrive\\Documentos\\DoomLevelEditorSaveFiles"));
            fileChooser.setDialogTitle("Specify a file to save");

            int userSelection = fileChooser.showSaveDialog(this);

            if (userSelection == JFileChooser.APPROVE_OPTION) {
                File fileToSave = fileChooser.getSelectedFile();
                try (FileWriter fileWriter = new FileWriter(fileToSave)) {
                    fileWriter.write(model.LinestoJSON().toString());
                } catch (IOException ioException) {
                    ioException.printStackTrace();
                }
            }
            
            /*
            int numColors = 512;
            	String palette = "const int numColors = " + numColors + ";\n";
            	palette += "const vec3 palette[numColors] = vec3[](\n";
                int index = 0;
                float stepCol = 8;
                for (int r = 0; r < stepCol; r++) {
                    for (int g = 0; g < stepCol; g++) {
                        for (int b = 0; b < stepCol; b++) {
                        	index++;
                            
                            if(index==numColors) {
                            	palette += "vec3("+ ((float)r / stepCol) + "f, " + ((float)g / stepCol) + "f, " + ((float)b / stepCol) + "f),\n";
                            } else {
                            	palette += "vec3("+ ((float)r / stepCol) + "f, " + ((float)g / stepCol) + "f, " + ((float)b / stepCol) + "f),\n";
                            }
                        }
                    }
                }
                palette+=");";
            
                System.out.println(palette);*/
            
		} else 		if(e.getSource()==btnSaveCpp) {
			// Save to file
			
			JFileChooser fileChooser = new JFileChooser();
			fileChooser.setCurrentDirectory(new File("C:\\Users\\joanc\\OneDrive\\Documentos\\DoomLevelEditorSaveFiles"));
            fileChooser.setDialogTitle("Specify a file to save");

            int userSelection = fileChooser.showSaveDialog(this);

            if (userSelection == JFileChooser.APPROVE_OPTION) {
                File fileToSave = fileChooser.getSelectedFile();
                try (FileWriter fileWriter = new FileWriter(fileToSave)) {
                	String s = " \" hello \" ";
                	fileWriter.write("#include\"Walls.h\" \n"
                			+ "float walls[] = {");
                	int i = 0;
                	for(Line l:model.getLines()) {
                		int ax = l.getPointA().getX();
                		int az = l.getPointA().getY();
                		int bx = l.getPointB().getX();
                		int bz = l.getPointB().getY();
                		int lx;
                		int rx;
                		int lz;
                		int rz;
                		if(ax==bx) {
                			// sort by z
                			if(az>bz) {
                				rx = ax*10 + 1;
                				rz = az * 10;
                				lx = bx*10 - 1;
                				lz = bz*10;
                			} else {
                				rx = bx*10 + 1;
                				rz = bz * 10;
                				lx = ax*10 - 1;
                				lz = az*10;
                			}
                		} else {
                			// sort by x
                			if(ax>bx) {
                				rx = ax*10;
                				rz = az*10 + 1;
                				lx = bx*10;
                				lz = bz*10 - 1;
                			} else {
                				rx = bx * 10;
                				rz = bz*10 + 1;
                				lx = ax*10;
                				lz = az*10 - 1;
                			}
                		}
                		if(i==model.getLines().size()-1) {
                			fileWriter.write(lx+", " + lz +", " + rx + ", " + rz + "\n");
                		} else {
                			fileWriter.write(lx+", " + lz +", " + rx + ", " + rz + ",\n");
                		}
                		i++;
                	}
                    fileWriter.write("};\n");
                    fileWriter.write("float* getWallAddress(int wallId) {\n"
                    		+ "	return &walls[wallId*4]; "
                    		+ "\n }");
                } catch (IOException ioException) {
                    ioException.printStackTrace();
                }
            }
		} else if(e.getSource()==btnLoad) {
			// Load file
			JFileChooser fileChooser = new JFileChooser();
			fileChooser.setCurrentDirectory(new File("C:\\Users\\joanc\\OneDrive\\Documentos\\DoomLevelEditorSaveFiles"));
            fileChooser.setDialogTitle("Select a file to load");

            int userSelection = fileChooser.showOpenDialog(this);

            if (userSelection == JFileChooser.APPROVE_OPTION) {
                File fileToLoad = fileChooser.getSelectedFile();
                
                System.out.println("Load file: " + fileToLoad.getAbsolutePath());

                // Add your code here to read data from the file
                try (BufferedReader reader = new BufferedReader(new FileReader(fileToLoad))) {
                    String line;
                    String jsonString = "";
                    while ((line = reader.readLine()) != null) {
                        jsonString += line +"\r\n";
                    }
                    JSONArray ja = new JSONArray(jsonString);
                    List<Object> objects = ja.toList();
            		List<Line> lines = new ArrayList<Line>();
            		for(Object o:objects) {
            			lines.add(Line.convertToLine( (Map<String, Object>) o));
            		}
            		
            		model.setLines(lines);
                } catch (IOException ioException) {
                    ioException.printStackTrace();
                }
                
                
            }
		} else if (e.getSource()==btnShow) {
			if (Desktop.isDesktopSupported()) {
				File fileToOpen = new File("C:\\Users\\joanc\\OneDrive\\Documentos\\DoomLevelEditorSaveFiles");
                try {
					Desktop.getDesktop().open(fileToOpen);
				} catch (IOException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
            } else {
                // Desktop is not supported
                JOptionPane.showMessageDialog(this, "Desktop is not supported on this system", "Error", JOptionPane.ERROR_MESSAGE);
            }
		} else if(e.getSource()==btnReset) {
			model.setLines(new ArrayList<Line>());
		} else if( e.getSource()==tfCols || (e.getSource()==tfRows)) {
			model.setRows(Integer.parseInt(tfRows.getText()));
			model.setCols(Integer.parseInt(tfCols.getText()));
		} else if(e.getSource() instanceof JButton &&(obstacleButton=isObstacleButton(e.getSource()))!=-1) {
			board.create(obstacleButton);
		}
	}


	public int isObstacleButton(Object but) {
		for(int i = 0; i < obstacleButtons.length; i++) {
			if(but==obstacleButtons[i]) {
				return i;
			}
		}
		
		return -1;
	}

	
	
}
