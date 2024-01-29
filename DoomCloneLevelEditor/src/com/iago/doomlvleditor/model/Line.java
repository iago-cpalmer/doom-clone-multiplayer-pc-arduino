package com.iago.doomlvleditor.model;

import java.util.HashMap;
import java.util.Map;

import org.json.JSONObject;

public class Line implements java.io.Serializable{
	/** Properties **/
	protected Vector2 pointA, pointB;
	protected int type;

	public Line(Vector2 pointA, Vector2 pointB, int type) {
		this.pointA = pointA;
		this.pointB = pointB;
		this.type = type;
	}

	public Vector2 getPointA() {
		return pointA;
	}

	public void setPointA(Vector2 pointA) {
		this.pointA = pointA;
	}

	public Vector2 getPointB() {
		return pointB;
	}

	public void setPointB(Vector2 pointB) {
		this.pointB = pointB;
	}
	
	public JSONObject toJSON() {
		JSONObject j = new JSONObject();
		j.put("Type", type);
		j.put("A", pointA.toJSON());
		j.put("B", pointB.toJSON());
		return j;
	}
	
	public static Line convertToLine(Map<String, Object> map) {
        @SuppressWarnings("unchecked")
        Map<String, Integer> aMap = (Map<String, Integer>) map.get("A");
        Vector2 a = new Vector2(aMap.get("X")/10, aMap.get("Y")/10);

        @SuppressWarnings("unchecked")
        Map<String, Integer> bMap = (Map<String, Integer>) map.get("B");
        Vector2 b = new Vector2(bMap.get("X")/10, bMap.get("Y")/10);

        // Optionally handle the 'Type' key here if needed
        int type = (int) map.get("Type");
        return new Line(a, b, type);        
    }

	
	public int getType() {
		return this.type;
	}
	
	@Override
	public String toString() {
		return "Line [pointA=" + pointA + ", pointB=" + pointB + ", type=" + type + "]";
	}

	
}
