module Building(x,y,z,s,c, callCount = 0, label="", secondlbl=""){
   		textSize = 6;
        translate([x,y,0])
        { 
        	// Cube
        	color(c){
        		if(s < 96)
    				translate([0,0,-1])
        			cube([s,s,z]);
        		else
        			cube([s,s,z]);
        	}
        	// Text
        	color([0,0,0,1]){
        		if(secondlbl == ""){
        			translate([2, s/2, z])
        			text(label, size = textSize);
        		}
        		else{
        			translate([2, s/2 + 5, z])
        			text(label, size = textSize);
        		}
        		
        		translate([2, s/2 - textSize, z])
        		text(secondlbl, size = textSize);

        		translate([4, s/2 + 20, z])
        		text(callCount, size = textSize);

        	}
    	}
}

module Road(point1, point2, size, roadColor = [0.5,0.5,0.5,1]){
		color(roadColor)	
		hull()
		{
			translate(point1) sphere(size);
			translate(point2) sphere(size);
		}
} 


module Arrow(point, angle, arrowColor, size=5){
	color(arrowColor)
	translate(point)
	rotate(angle - 90)
	translate([-size * 2, -size * 2, -size * 0.5])
	polyhedron
    (points = [
    		[0,0,0],[2 * size, 1 * size, 0],[4 * size, 0,0],
    		[2 * size, 5 * size, 0],[2 * size, 2 * size, size]
    	], 
     faces = [
		  	[0,1,4],[0,1,3],[0,4,3],[2,1,4],[2,1,3],[2,4,3]
		  ]
     );
}


