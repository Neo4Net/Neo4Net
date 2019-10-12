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
namespace Org.Neo4j.Kernel.impl.storemigration
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// Different operations on a file, for example copy or move, given a <seealso cref="FileSystemAbstraction"/> and
	/// source/destination.
	/// </summary>
	public abstract class FileOperation
	{
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       COPY { public void perform(org.neo4j.io.fs.FileSystemAbstraction fs, String fileName, java.io.File fromDirectory, boolean skipNonExistentFromFile, java.io.File toDirectory, ExistingTargetStrategy existingTargetStrategy) throws java.io.IOException { java.io.File fromFile = fromFile(fs, fromDirectory, fileName, skipNonExistentFromFile); if(fromFile != null) { java.io.File toFile = toFile(fs, toDirectory, fileName, existingTargetStrategy); if(toFile != null) { fs.copyFile(fromFile, toFile); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       MOVE { public void perform(org.neo4j.io.fs.FileSystemAbstraction fs, String fileName, java.io.File fromDirectory, boolean skipNonExistentFromFile, java.io.File toDirectory, ExistingTargetStrategy existingTargetStrategy) throws java.io.IOException { java.io.File fromFile = fromFile(fs, fromDirectory, fileName, skipNonExistentFromFile); if(fromFile != null) { if(toFile(fs, toDirectory, fileName, existingTargetStrategy) != null) { fs.moveToDirectory(fromFile, toDirectory); } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//       DELETE { public void perform(org.neo4j.io.fs.FileSystemAbstraction fs, String fileName, java.io.File directory, boolean skipNonExistentFromFile, java.io.File unusedFile, ExistingTargetStrategy unused) { java.io.File file = fromFile(fs, directory, fileName, skipNonExistentFromFile); if(file != null) { fs.deleteFile(file); } } };

		 private static readonly IList<FileOperation> valueList = new List<FileOperation>();

		 static FileOperation()
		 {
			 valueList.Add( COPY );
			 valueList.Add( MOVE );
			 valueList.Add( DELETE );
		 }

		 public enum InnerEnum
		 {
			 COPY,
			 MOVE,
			 DELETE
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 private FileOperation( string name, InnerEnum innerEnum )
		 {
			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract void perform(org.neo4j.io.fs.FileSystemAbstraction fs, String fileName, java.io.File fromDirectory, boolean skipNonExistentFromFile, java.io.File toDirectory, ExistingTargetStrategy existingTargetStrategy) throws java.io.IOException;
		 public abstract void perform( Org.Neo4j.Io.fs.FileSystemAbstraction fs, string fileName, java.io.File fromDirectory, bool skipNonExistentFromFile, java.io.File toDirectory, ExistingTargetStrategy existingTargetStrategy );

		 public static readonly FileOperation private static java.io.File fromFile( Org.Neo4j.Io.fs.FileSystemAbstraction fs, java.io.File directory, String name, boolean skipNonExistent )
		 {
			 java.io.File fromFile = new java.io.File( directory, name ); if ( skipNonExistent && !fs.fileExists( fromFile ) ) { return null; } return fromFile;
		 }
		 private static java.io.File toFile( Org.Neo4j.Io.fs.FileSystemAbstraction fs, java.io.File directory, String name, ExistingTargetStrategy existingTargetStrategy )
		 {
			 java.io.File file = new java.io.File( directory, name ); if ( fs.fileExists( file ) )
			 {
				 switch ( existingTargetStrategy ) { case FAIL: case OVERWRITE: fs.deleteFile( file ); return file; case SKIP: return null; default: throw new IllegalStateException( existingTargetStrategy.name() ); }
			 }
			 return file;
		 }
		 = new FileOperation("private static java.io.File fromFile(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File directory, String name, boolean skipNonExistent) { java.io.File fromFile = new java.io.File(directory, name); if(skipNonExistent && !fs.fileExists(fromFile)) { return null; } return fromFile; } private static java.io.File toFile(org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File directory, String name, ExistingTargetStrategy existingTargetStrategy) { java.io.File file = new java.io.File(directory, name); if(fs.fileExists(file)) { switch(existingTargetStrategy) { case FAIL: case OVERWRITE: fs.deleteFile(file); return file; case SKIP: return null; default: throw new IllegalStateException(existingTargetStrategy.name()); } } return file; }", InnerEnum.private static java.io.File fromFile(Org.Neo4j.Io.fs.FileSystemAbstraction fs, java.io.File directory, String name, boolean skipNonExistent)
		 {
			 java.io.File fromFile = new java.io.File( directory, name ); if ( skipNonExistent && !fs.fileExists( fromFile ) ) { return null; } return fromFile;
		 }
		 private static java.io.File toFile( Org.Neo4j.Io.fs.FileSystemAbstraction fs, java.io.File directory, String name, ExistingTargetStrategy existingTargetStrategy )
		 {
			 java.io.File file = new java.io.File( directory, name ); if ( fs.fileExists( file ) )
			 {
				 switch ( existingTargetStrategy ) { case FAIL: case OVERWRITE: fs.deleteFile( file ); return file; case SKIP: return null; default: throw new IllegalStateException( existingTargetStrategy.name() ); }
			 }
			 return file;
		 });

		public static IList<FileOperation> values()
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

		public static FileOperation valueOf( string name )
		{
			foreach ( FileOperation enumInstance in FileOperation.valueList )
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