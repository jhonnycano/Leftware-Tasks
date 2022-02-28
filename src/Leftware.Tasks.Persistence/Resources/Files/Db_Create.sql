CREATE TABLE [col_header] (
	-- [id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[name] varchar(40) NOT NULL PRIMARY KEY, 
	[itemType] varchar(30) NOT NULL, 
	[schema] varchar(4000) NULL
);

CREATE TABLE [col_item] (
	[id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[col] varchar(40) NOT NULL, 
	[key] varchar(60) NOT NULL, 
	[label] varchar(60) NOT NULL, 
	[content] varchar(4000) NOT NULL, 
	UNIQUE ([col], [key]) ON CONFLICT REPLACE, 
	FOREIGN KEY (col) REFERENCES col_header ([name])
);
