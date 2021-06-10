CREATE DATABASE  IF NOT EXISTS `mysite` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `mysite`;

--
-- Table structure for table `accountbal`
--

DROP TABLE IF EXISTS `accountbal`;
CREATE TABLE `accountbal` (
  `UUID` varchar(36) NOT NULL,
  `Name` varchar(64) NOT NULL,
  `Action` varchar(50) NOT NULL,
  `TransDate` datetime NOT NULL,
  `Amount` decimal(10,4) NOT NULL,
  `Actual` decimal(10,4) NOT NULL,
  `TransFee` decimal(10,4) NOT NULL,
  `ExchangeRate` int(11) NOT NULL DEFAULT '0',
  `txnID` varchar(50) NOT NULL,
  KEY `UUID` (`UUID`),
  KEY `TransDate` (`TransDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `contactus`
--

DROP TABLE IF EXISTS `contactus`;
CREATE TABLE `contactus` (
  `ContactID` int(11) NOT NULL AUTO_INCREMENT,
  `SendEmail` varchar(50) NULL,
  `Title` varchar(50) NULL,
  `Subject` varchar(60) NULL,
  `AutoResponse` varchar(5000) NOT NULL DEFAULT '',
  `SortOrder` float NOT NULL DEFAULT '999.0',
  `Active` enum('true','false') NOT NULL DEFAULT 'false',
  PRIMARY KEY (`ContactID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `control`
--

DROP TABLE IF EXISTS `control`;
CREATE TABLE `control` (
  `Control` varchar(20) NOT NULL,
  `Name` varchar(30) NOT NULL,
  `Parm1` varchar(20) NOT NULL,
  `Parm2` varchar(150) NULL,
  `Nbr1` int(11) NOT NULL DEFAULT '0',
  `Nbr2` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Control`,`Name`),
  INDEX `Parm1` (`Parm1` ASC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `pagemaster`
--

DROP TABLE IF EXISTS `pagedetail`;
DROP TABLE IF EXISTS `pagemaster`;
CREATE TABLE `pagemaster` (
  `PageID`  int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  `Path` varchar(2048) NOT NULL,
  PRIMARY KEY (`PageID`),
  KEY `IDX_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `pagedetail`
--

CREATE TABLE `pagedetail` (
  `EntryID`  int(11) NOT NULL AUTO_INCREMENT,
  `PageID`  int(11) NOT NULL,
  `SortOrder` float NOT NULL DEFAULT '999.0',
  `Name` varchar(50) NOT NULL,
  `Title` varchar(100) NOT NULL,
  `Content` varchar(2048) NOT NULL,
  `Active` enum('true','false') NOT NULL DEFAULT 'false',
  `AutoStart` datetime NULL,
  `AutoExpire` datetime NULL,
  PRIMARY KEY (`EntryID`),
  FOREIGN KEY (PageID) REFERENCES pagemaster(PageID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `regionxml`
--

DROP TABLE IF EXISTS `regionxml`;
CREATE TABLE `regionxml` (
  `UUID` varchar(36) NOT NULL,
  `regionName` varchar(32) NOT NULL,
  `status` int(1) NOT NULL DEFAULT '0',
  `locationX` int(10) NOT NULL,
  `locationY` int(10) NOT NULL,
  `internalIP` varchar(64) NOT NULL,
  `port` int(10) NOT NULL,
  `externalIP` varchar(64) NOT NULL,
  `ownerUUID` varchar(36) NOT NULL,
  `lastmapUUID` varchar(36) NOT NULL,
  `lastmapRefresh` varchar(64) NOT NULL,
  `primMax` int(10) NOT NULL,
  `physicalMax` int(10) NOT NULL,
  `productType` int(1) NOT NULL,
  `CreateDate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `MachineName` varchar(50) NOT NULL DEFAULT '',
  `ProcessHandle` int(10) NOT NULL DEFAULT '0',
  PRIMARY KEY (`UUID`),
  KEY `regionName` (`regionName`),
  KEY `locationX` (`locationX`),
  KEY `locationY` (`locationY`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Table structure for table `usereconomy`
--

DROP TABLE IF EXISTS `usereconomy`;
CREATE TABLE `usereconomy` (
  `UUID` varchar(36) NOT NULL,
  `MaxBuy` int(11) NOT NULL DEFAULT '0',
  `MaxSell` int(11) NOT NULL DEFAULT '0',
  `Hours` int(11) NOT NULL DEFAULT '0',
  PRIMARY KEY (`UUID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
