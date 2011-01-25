<?php

error_reporting(E_ALL | E_STRICT);

$baseurl = "/home/julianapena/winmilk.julianapena.com";
/*$imageuri = "/tiles/Tile0.png";

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
*/


if (isset($_REQUEST["count"])) {
	$number = (int) $_REQUEST["count"];
}
else {
	$number = 0;
}

$width = 173;
$height = 173;

$image = imagecreatetruecolor($width, $height);
imagealphablending($image,true);
imagesavealpha($image,true);

$trans = imagecolorallocatealpha($image, 0, 0, 0, 127);
$white = imagecolorallocate($image, 0, 0, 255);
$black = imagecolorallocate($image, 0, 0, 0);

imagealphablending($image,false);
imagefill($image, 0, 0, $trans);
imagealphablending($image,true);

$icon = imagecreatefrompng("$baseurl/numbers/icon.png");
$w = imagesx($icon);
$h = imagesy($icon);

if ($number <= 0) {
	imagecopy($image, $icon, 45, 40, 0, 0, $w, $h);
}
else if ($number < 10) {
	imagecopy($image, $icon, 30, 40, 0, 0, $w, $h);
	
	$count = imagecreatefrompng("$baseurl/numbers/".$_REQUEST["count"].".png");
	$w = imagesx($count);
	$h = imagesy($count);
	
	imagecopy($image, $count, 95, 58, 0, 0, $w, $h);
}
else {
	imagecopy($image, $icon, 20, 40, 0, 0, $w, $h);
	
	$count = imagecreatefrompng("$baseurl/numbers/9plus.png");
	$w = imagesx($count);
	$h = imagesy($count);
	
	imagecopy($image, $count, 85, 58, 0, 0, $w, $h);
	
	
}

header("Content-type: image/png");
imagepng($image);

?>