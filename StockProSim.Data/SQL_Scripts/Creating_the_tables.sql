CREATE TABLE Client(
	UserID int Primary Key,
    UserFirstName varchar(255),
    UserLastName varchar(255),
	UserBalance int
);
CREATE TABLE Watch_List(
	StockID int Primary Key,
    StockTicker varchar(255)
);
CREATE TABLE Trade (
	SaleID int Primary Key,
	StockTicker varchar(255),
    StockPrice int,
    StockQuantity int,
	DateOfSale date,
	TimeOfSale time
);
CREATE TABLE Trader (
	TradeID int Primary Key,
    UserID int,
    TradeWorth int,
	DateOfTrade date,
	TimeOfTrade time,
    FOREIGN KEY (UserID) REFERENCES Client(UserID)
);

