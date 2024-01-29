package com.iago.doomlvleditor.view;

public class PaintThread implements Runnable{
	private View view;
	private final int FPS = 60;
	public PaintThread(View view) {
		this.view = view;
	}
	@Override
	public void run() {
		long lastTime = System.nanoTime();
		long timeSpan = (long) ((1/FPS)*Math.pow(10, 9));
		
		while(true) {
			if(System.nanoTime() - lastTime >= timeSpan) {
				view.repaint();
				lastTime = System.nanoTime();
			}
		}
		
	}
	
}
