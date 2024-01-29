package com.iago.doomlvleditor.model;

import org.json.JSONObject;

public class Vector2 implements java.io.Serializable{
	/** Properties **/
	private int x,y;
	
	public Vector2(int x, int y) {
		this.x=x;
		this.y=y;
	}

	public int getX() {
		return x;
	}

	public void setX(int x) {
		this.x = x;
	}

	public int getY() {
		return y;
	}

	public void setY(int y) {
		this.y = y;
	}
	
	public boolean equals(Vector2 b) {
		return b.getX()==this.getX() && b.getY()==this.getY();
	}
	
	public String toString() {
		return "(" + x + ", " + y +")";
	}
	
	public void add(Vector2 b) {
		this.x=this.x+b.getX();
		this.y=this.y + b.getY();
	}
	
	public boolean equals(int x, int y) {
		return this.x==x && this.y==y;
	}
	
	public double distance(Vector2 b) {
		return Math.sqrt(((this.x-b.getX())*(this.x-b.getX()) + 
				(this.y-b.getY())*(this.y-b.getY())));
	}
	
	public Vector2 clone() {
		return new Vector2(x,y);
	}
	
	public JSONObject toJSON() {
		JSONObject j = new JSONObject();
		j.put("X", x*10);
		j.put("Y", y*10);
		return j;
	}
}
