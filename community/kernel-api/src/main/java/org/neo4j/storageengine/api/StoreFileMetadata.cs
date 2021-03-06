﻿/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Storageengine.Api
{

	public class StoreFileMetadata
	{
		 private readonly File _file;
		 private readonly int _recordSize;
		 private readonly bool _isLogFile;

		 public StoreFileMetadata( File file, int recordSize ) : this( file, recordSize, false )
		 {
		 }

		 public StoreFileMetadata( File file, int recordSize, bool isLogFile )
		 {
			  this._file = file;
			  this._recordSize = recordSize;
			  this._isLogFile = isLogFile;
		 }

		 public virtual File File()
		 {
			  return _file;
		 }

		 public virtual int RecordSize()
		 {
			  return _recordSize;
		 }

		 public virtual bool LogFile
		 {
			 get
			 {
				  return _isLogFile;
			 }
		 }
	}

}