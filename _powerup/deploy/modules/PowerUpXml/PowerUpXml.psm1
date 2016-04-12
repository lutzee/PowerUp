## Some function code /ideas acquired from http://blog.danskingdom.com/powershell-functions-to-get-an-xml-node-and-get-and-set-an-xml-elements-value-even-when-the-element-does-not-already-exist/

function Read-XMLValue($filename, $xpath, $element) {
	$file = get-item $filename;
	$x = [xml] (Get-Content $file)
	Select-Xml -xml $x  -XPath $xpath |
    % {
        $value = $_.Node.$element;
		return $value;
      }
}


function Write-XMLValue($filename, $xpath, $element, $filenameout, $value) {

	if(-not(Test-Path $filename)){
		throw " file $filename does not exist and is required";
	};

	[xml]$x = Get-Content -Path $filename

	
	$localxpath = $xpath -replace '\[\d{1}\]' ,  '';
	
	$nodes = $localxpath -split "/"
	foreach($nod in $nodes)
	{	
		if ($nod) {
		
			if ($currentxpath) {
				$currentxpath += "/" + $nod  
			}
			else {
				$currentxpath = $nod  
			}
			
			Set-XmlElementsTextValue -XmlDocument $x -ElementPath $currentxpath -TextValue ""
		}
	}

	if($element)
	{
		Select-Xml -xml $x  -XPath $xpath |
		% {
			$_.Node.SetAttribute($element,$value)
		}
	}
	$x.Save($filenameout)
	
}


function Remove-XMLNode($filename, $xpath, $filenameout) {
	$file = get-item $filename;
	$x = [xml] (Get-Content $file)
	
	Select-Xml -xml $x  -XPath $xpath |
    % {
		$_.Node.ParentNode.RemoveChild($_.Node);
      }
	$x.Save($filenameout);
}

function Get-XmlNamespaceManager([xml]$XmlDocument, [string]$NamespaceURI = "")
{
    # If a Namespace URI was not given, use the Xml document's default namespace.
    if ([string]::IsNullOrEmpty($NamespaceURI)) { $NamespaceURI = $XmlDocument.DocumentElement.NamespaceURI }   
     
    # In order for SelectSingleNode() to actually work, we need to use the fully qualified node path along with an Xml Namespace Manager, so set them up.
    [System.Xml.XmlNamespaceManager]$xmlNsManager = New-Object System.Xml.XmlNamespaceManager($XmlDocument.NameTable)
    $xmlNsManager.AddNamespace("ns", $NamespaceURI)
    return ,$xmlNsManager       # Need to put the comma before the variable name so that PowerShell doesn't convert it into an Object[].
}
 
function Get-FullyQualifiedXmlNodePath([string]$NodePath, [string]$NodeSeparatorCharacter = '.')
{
    return "/ns:$($NodePath.Replace($($NodeSeparatorCharacter), '/ns:'))"
}


function Get-XmlNode([xml]$XmlDocument, [string]$NodePath, [string]$NamespaceURI = "", [string]$NodeSeparatorCharacter = '/')
{
    $xmlNsManager = Get-XmlNamespaceManager -XmlDocument $XmlDocument -NamespaceURI $NamespaceURI
    [string]$fullyQualifiedNodePath = Get-FullyQualifiedXmlNodePath -NodePath $NodePath -NodeSeparatorCharacter $NodeSeparatorCharacter
     
    # Try and get the node, then return it. Returns $null if the node was not found.
    $node = $XmlDocument.SelectSingleNode($fullyQualifiedNodePath, $xmlNsManager)
    return $node
}
 
function Get-XmlNodes([xml]$XmlDocument, [string]$NodePath, [string]$NamespaceURI = "", [string]$NodeSeparatorCharacter = '/')
{
    $xmlNsManager = Get-XmlNamespaceManager -XmlDocument $XmlDocument -NamespaceURI $NamespaceURI
    [string]$fullyQualifiedNodePath = Get-FullyQualifiedXmlNodePath -NodePath $NodePath -NodeSeparatorCharacter $NodeSeparatorCharacter
 
    # Try and get the nodes, then return them. Returns $null if no nodes were found.
    $nodes = $XmlDocument.SelectNodes($fullyQualifiedNodePath, $xmlNsManager)
    return $nodes
}
 
function Set-XmlElementsTextValue([xml]$XmlDocument, [string]$ElementPath, [string]$TextValue, [string]$NamespaceURI = "", [string]$NodeSeparatorCharacter = '/')
{
    # Try and get the node. 
    $node = Get-XmlNode -XmlDocument $XmlDocument -NodePath $ElementPath -NamespaceURI $NamespaceURI -NodeSeparatorCharacter $NodeSeparatorCharacter
     
    # If the node already exists, update its value.
    if ($node)
    { 
		if ($textValue) {
			$node.InnerText = $TextValue
		}
		else {
			#Write-Output "dont update as textvale is null";
		}
    }
    # Else the node doesn't exist yet, so create it with the given value.
    else
    {
        # Create the new element with the given value.
        $elementName = $ElementPath.SubString($ElementPath.LastIndexOf($NodeSeparatorCharacter) + 1)
        $element = $XmlDocument.CreateElement($elementName, $XmlDocument.DocumentElement.NamespaceURI)      
        $textNode = $XmlDocument.CreateTextNode($TextValue)
        $element.AppendChild($textNode) > $null
         
        # Try and get the parent node.
        $parentNodePath = $ElementPath.SubString(0, $ElementPath.LastIndexOf($NodeSeparatorCharacter))
        $parentNode = Get-XmlNode -XmlDocument $XmlDocument -NodePath $parentNodePath -NamespaceURI $NamespaceURI -NodeSeparatorCharacter $NodeSeparatorCharacter
         
        if ($parentNode)
        {
            $parentNode.AppendChild($element) > $null
        }
        else
        {
            throw "$parentNodePath does not exist in the xml."
        }
    }
}

function Write-XMLValueKey($filename, $xpath, $element, $filenameout, $value, $keyname, $key ) {

	[xml]$x = Get-Content -Path $filename
	
	$compositexpath = $xpath+"[@"+$keyname+"='"+$key+"']"
	$relevantNode = $x.SelectSingleNode($compositexpath)
    
	if ($relevantNode -ne $null)
    {
        # Just Update 
     	$relevantNode.SetAttribute($element,$value)
    }
	else 
	{
		#create and update 
		$localxpath = $xpath -replace '\[\d{1}\]' ,  '';
		$nodes = $localxpath -split "/"
		
		$i = 0;
		do 
		{
		
			if ($nodes[$i]) {
				$xpathminus1 += "/" + $nodes[$i]
				if ($currentxpath) {
					$currentxpath += "/" + $nodes[$i]  
				}
				else {
					$currentxpath = $nodes[$i]  
				}
			
				Set-XmlElementsTextValue -XmlDocument $x -ElementPath $currentxpath -TextValue ""
			}
			
			$i++;
		} while ($i -lt ($nodes.Count -1))
		
		$xpathlast = $nodes[$nodes.Count-1];
		$parentNode = $x.SelectSingleNode($xpathminus1)
		
		$root = $x.get_DocumentElement();    
		$relevantNode = $x.CreateNode('element', $xpathlast,"")    
		$relevantNode.SetAttribute($keyname, $key)
		if($element)
		{
			$relevantNode.SetAttribute($element, $value )
		}
		$rNode =  $x.SelectSingleNode($xpathminus1).AppendChild($relevantNode)
	}
		
	$x.Save($filenameout)

}


export-modulemember -function Read-XMLValue, Write-XMLValue, Remove-XMLNode, Write-XMLValueKey