package com.iago.doomlvleditor.model;

import java.awt.Color;
import java.util.ArrayList;
import java.util.List;

import org.json.JSONArray;


public class Model {
	private int cols, rows;

	private List<Line> lines;
	
	public String[] types = {"Wall", "Door"};
	public Color[] obstacleColors = {Color.black, Color.pink};
	public Model(int cols, int rows, int xPos, int yPos) {
		this.cols = cols;
		this.rows=rows;
		lines = new ArrayList<Line>();
	}

	public int getCols() {
		return cols;
	}

	public void setCols(int cols) {
		this.cols = cols>=0?cols:1;
	}

	public int getRows() {
		return rows;
	}

	public void setRows(int rows) {
		this.rows=rows>=0?rows:1;
	}

	public List<Line> getLines() {
		return lines;
	}

	public void setLines(List<Line> lines) {
		this.lines = lines;
	}
	
	public JSONArray LinestoJSON() {
		JSONArray ja = new JSONArray();
		for(Line l:lines) {
			ja.put(l.toJSON());
		}
		return ja;
	}
}
