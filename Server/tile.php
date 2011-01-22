<?php

$baseurl = "/home/julianapena/winmilk.julianapena.com";
$imageuri = "/tiles/Tile0.png";

if (isset($_REQUEST["count"]))
{
	$count = (int)$_REQUEST["count"];
	
	if ($count >= 10)
	{
		$imageuri = "/tiles/Tile9p.png";
	}
	else if ($count <= 9 && $count > 0)
	{
		$imageuri = "/tiles/Tile$count.png";
	}
}

header("Content-type: image/png");
$im = imagecreatefrompng($baseurl.$imageuri);
imagepng($im);
?>