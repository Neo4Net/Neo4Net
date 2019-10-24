using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.storemigration
{

	using Neo4Net.Collections.Helpers;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;

	internal sealed class MigrationStatus
	{
		 public static readonly MigrationStatus Migrating = new MigrationStatus( "Migrating", InnerEnum.Migrating );
		 public static readonly MigrationStatus Moving = new MigrationStatus( "Moving", InnerEnum.Moving );
		 public static readonly MigrationStatus Completed = new MigrationStatus( "Completed", InnerEnum.Completed );

		 private static readonly IList<MigrationStatus> valueList = new List<MigrationStatus>();

		 static MigrationStatus()
		 {
			 valueList.Add( Migrating );
			 valueList.Add( Moving );
			 valueList.Add( Completed );
		 }

		 public enum InnerEnum
		 {
			 Migrating,
			 Moving,
			 Completed
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private MigrationStatus( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 public bool IsNeededFor( MigrationStatus current )
		 {
			  return current == null || this.ordinal() >= current.ordinal();
		 }

		 public string MaybeReadInfo( Neo4Net.Io.fs.FileSystemAbstraction fs, java.io.File stateFile, string currentInfo )
		 {
			  if ( !string.ReferenceEquals( currentInfo, null ) )
			  {
					return currentInfo;
			  }

			  Pair<string, string> data = ReadFromFile( fs, stateFile, this );
			  return data == null ? null : data.Other();
		 }

		 public static MigrationStatus ReadMigrationStatus( Neo4Net.Io.fs.FileSystemAbstraction fs, java.io.File stateFile )
		 {
			  Pair<string, string> data = ReadFromFile( fs, stateFile, null );
			  if ( data == null )
			  {
					return null;
			  }

			  return MigrationStatus.ValueOf( data.First() );
		 }

		 private static Neo4Net.Collections.Helpers.Pair<string, string> ReadFromFile( Neo4Net.Io.fs.FileSystemAbstraction fs, java.io.File file, MigrationStatus expectedSate )
		 {
			  try
			  {
					  using ( StreamReader reader = new StreamReader( fs.OpenAsReader( file, StandardCharsets.UTF_8 ) ) )
					  {
						string state = reader.ReadLine().Trim();
						if ( expectedSate != null && !expectedSate.name().Equals(state) )
						{
							 throw new System.InvalidOperationException( "Not in the expected state, expected=" + expectedSate.name() + ", actual=" + state );
						}
						string info = reader.ReadLine().Trim();
						return Pair.of( state, info );
					  }
			  }
			  catch ( FileNotFoundException )
			  {
					return null;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public void SetMigrationStatus( Neo4Net.Io.fs.FileSystemAbstraction fs, java.io.File stateFile, string info )
		 {
			  if ( fs.FileExists( stateFile ) )
			  {
					try
					{
						 fs.Truncate( stateFile, 0 );
					}
					catch ( IOException e )
					{
						 throw new Exception( e );
					}
			  }

			  try
			  {
					  using ( Writer writer = fs.OpenAsWriter( stateFile, StandardCharsets.UTF_8, false ) )
					  {
						writer.write( name() );
						writer.write( '\n' );
						writer.write( info );
						writer.flush();
					  }
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		public static IList<MigrationStatus> values()
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

		public static MigrationStatus ValueOf( string name )
		{
			foreach ( MigrationStatus enumInstance in MigrationStatus.valueList )
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