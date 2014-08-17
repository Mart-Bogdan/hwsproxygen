module SoapHttpClientProtocol (
   invokeWS,
   Parameter,
   primitiveToStr,
   xmlTagStart, xmlTagEnd, buildXml,
   buildPrimitiveXmlList, buildComplexXmlList,
   getNodeValues,
   XmlSerializable(..),
   lowerFirstChar, upperFirstChar, headStr
) where

import Network.URI
import Data.Maybe (fromMaybe)
import System.IO
import Network.HTTP
import Network.Browser
import Network (withSocketsDo)
import Data.Char

type Parameter = (String,String) -- name and value

class XmlSerializable t where
     toXml :: t -> String
     fromXml :: String -> t
     
xmlTagStart :: String -> String
xmlTagStart s = '<':s ++ ['>']

xmlTagEnd :: String -> String
xmlTagEnd s = '<':'/':s ++ ['>']

buildXml :: String -> String -> String
buildXml elemName elemValue = (xmlTagStart elemName) ++ elemValue ++ (xmlTagEnd elemName)

buildPrimitiveXmlList :: Show t => [t] -> String -> String
buildPrimitiveXmlList elems elemsName =	foldr (++) [] (map ((buildXml elemsName).primitiveToStr) elems)

buildComplexXmlList :: XmlSerializable t => [t] -> String -> String
buildComplexXmlList elems elemsName = foldr (++) [] (map ((buildXml elemsName).toXml) elems)

primitiveToStr :: Show t => t -> String
primitiveToStr t
	| head str == '"' || head str == '\'' = (tail (take (length str - 1) str))
	| otherwise = str
	where str = show t

soapXmlHeader :: String
soapXmlHeader = "<?xml version='1.0' encoding='utf-8'?>" ++
                "<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'> " ++
                "<soap:Body>"

soapXmlFooter :: String
soapXmlFooter = "</soap:Body>" ++
		        "</soap:Envelope>"

functionToXml :: String -> String -> [Parameter] -> String
functionToXml name namespace parameters = 
	"<" ++ name ++ " xmlns=\'" ++ namespace ++ "\'>" ++
	parametersToXml parameters ++
	"</" ++ name ++ ">"

parametersToXml :: [Parameter] -> String
parametersToXml [] = []
parametersToXml ((name,value):ps) = 
	"<" ++ name ++ ">" ++  value ++ "</" ++ name ++ ">" ++
	parametersToXml ps

invokeWS :: String -> String -> String -> String -> [Parameter] -> String -> IO String
invokeWS uriStr methodName soapAction namespace parameters response = do
	xmlResp <- browse $ fn methodName soapAction namespace parameters
	-- putStrLn $ "in: " ++ contentTst
	-- putStrLn $ "out: " ++ xmlResp
	return $ head $ (getNodeValues xmlResp response)
	where uri = fromMaybe (error "Nothing from url parse") (parseURI uriStr)
	      contentTst = soapXmlHeader
	                   ++ (functionToXml methodName namespace parameters)
	                   ++ soapXmlFooter
	      req = buildSOAPRequest uri soapAction contentTst
	      fn methodName soapAction namespace parameters = do
			setCookieFilter (\_ _ -> return True)
			setErrHandler (\_ -> return ())
			setOutHandler (\_ -> return ())
			rsp <- request req
			return (rspBody $ snd rsp)

-- get the values of xml nodes specified by their name
-- ex.: getNodeValues "<a>1</a><b>2</b><a>3</a>" a = [1,3]
-- ex.: getNodeValues "<a>1</a><b>2</b><a>3</a>" b = [2]
getNodeValues :: String -> String -> [String]
getNodeValues [] nodeName = []
getNodeValues xml nodeName
	| (take (length xmlElement) xml) == xmlElement
		= let
			(nodeValue, restXml) = buildNodeValue (drop (length xmlElement) xml) nodeName []
          in
             nodeValue:(getNodeValues restXml nodeName)
	| otherwise = getNodeValues (tail xml) nodeName
	where xmlElement = xmlTagStart nodeName

-- returns the value of the xml element and the rest of the xml being parsed
buildNodeValue :: String -> String -> String -> (String, String)
buildNodeValue restXml nodeName accum
	| (take (length xmlElement) restXml) == xmlElement = (reverse accum, drop (length xmlElement) restXml)
	| otherwise = buildNodeValue (tail restXml) nodeName ((head restXml):accum)
	where xmlElement = xmlTagEnd nodeName

buildSOAPRequest uri action content =
    Request { rqURI=uri
            , rqBody=content
            , rqHeaders=[ Header HdrContentType "text/xml; charset=utf-8"
                        , Header HdrContentLength (show (length content))
                        , Header HdrUserAgent libUA
                        , Header (HdrCustom "SOAPAction") action
                        ]
            , rqMethod=POST
            }

-------------------------------------
-- Helper text manipulation functions
-------------------------------------

lowerFirstChar :: String -> String
lowerFirstChar (a:as) = (toLower a):as

upperFirstChar :: String -> String
upperFirstChar (a:as) = (toUpper a):as

headStr :: [String] -> String
headStr [] = ""
headStr (a:as) = a