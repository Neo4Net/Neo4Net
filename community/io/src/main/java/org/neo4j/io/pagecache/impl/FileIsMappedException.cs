using System.Collections.Generic;

/*
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
namespace Org.Neo4j.Io.pagecache.impl
{

	public class FileIsMappedException : IOException
	{
		 private readonly File _file;
		 private readonly Operation _operation;

		 public FileIsMappedException( File file, Operation operation ) : base( operation.message + ": " + file )
		 {
			  this._file = file;
			  this._operation = operation;
		 }

		 public virtual File File
		 {
			 get
			 {
				  return _file;
			 }
		 }

		 public virtual Operation GetOperation()
		 {
			  return _operation;
		 }

		 public sealed class Operation
		 {
			  public static readonly Operation Rename = new Operation( "Rename", InnerEnum.Rename, "Cannot rename mapped file" );
			  public static readonly Operation Delete = new Operation( "Delete", InnerEnum.Delete, "Cannot delete mapped file" );

			  private static readonly IList<Operation> valueList = new List<Operation>();

			  static Operation()
			  {
				  valueList.Add( Rename );
				  valueList.Add( Delete );
			  }

			  public enum InnerEnum
			  {
				  Rename,
				  Delete
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  internal Private readonly;

			  internal Operation( string name, InnerEnum innerEnum, string message )
			  {
					this._message = message;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<Operation> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Operation valueOf( string name )
			 {
				 foreach ( Operation enumInstance in Operation.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }
	}

}