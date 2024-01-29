package com.iago.doomlvleditor;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONObject;

import com.iago.doomlvleditor.model.Line;
import com.iago.doomlvleditor.model.Model;
import com.iago.doomlvleditor.model.Vector2;
import com.iago.doomlvleditor.view.PaintThread;
import com.iago.doomlvleditor.view.View;
public class Main {

	private static Model model;
	private static View view;
	
	private static final int COLS = 50;
	private static final int ROWS = 50;
	
	public static void main(String[] args) {
		model = new Model(COLS, ROWS, 1,1);
		view = new View(model);
		(new PaintThread(view)).run();
	}
	
}
