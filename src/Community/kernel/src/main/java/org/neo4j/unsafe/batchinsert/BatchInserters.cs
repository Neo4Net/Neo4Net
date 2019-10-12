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
namespace Neo4Net.@unsafe.Batchinsert
{

	using Service = Neo4Net.Helpers.Service;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using Neo4Net.Kernel.extension;
	using BatchInserterImpl = Neo4Net.@unsafe.Batchinsert.@internal.BatchInserterImpl;
	using FileSystemClosingBatchInserter = Neo4Net.@unsafe.Batchinsert.@internal.FileSystemClosingBatchInserter;
	using IndexConfigStoreProvider = Neo4Net.@unsafe.Batchinsert.@internal.IndexConfigStoreProvider;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

	/// <summary>
	/// Provides instances of <seealso cref="BatchInserter"/>.
	/// </summary>
	public sealed class BatchInserters
	{
		 /// <summary>
		 /// Get a <seealso cref="BatchInserter"/> given a store directory.
		 /// </summary>
		 /// <param name="databaseDirectory"> directory where particular neo4j database is located </param>
		 /// <returns> a new <seealso cref="BatchInserter"/> </returns>
		 /// <exception cref="IOException"> if there is an IO error </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BatchInserter inserter(java.io.File databaseDirectory) throws java.io.IOException
		 public static BatchInserter Inserter( File databaseDirectory )
		 {
			  DefaultFileSystemAbstraction fileSystem = CreateFileSystem();
			  BatchInserter batchInserter = Inserter( databaseDirectory, fileSystem, stringMap() );
			  return new FileSystemClosingBatchInserter( batchInserter, ( IndexConfigStoreProvider ) batchInserter, fileSystem );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BatchInserter inserter(java.io.File databaseDirectory, org.neo4j.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 public static BatchInserter Inserter( File databaseDirectory, FileSystemAbstraction fs )
		 {
			  return Inserter( databaseDirectory, fs, stringMap(), LoadKernelExtension() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BatchInserter inserter(java.io.File databaseDirectory, java.util.Map<String,String> config) throws java.io.IOException
		 public static BatchInserter Inserter( File databaseDirectory, IDictionary<string, string> config )
		 {
			  DefaultFileSystemAbstraction fileSystem = CreateFileSystem();
			  BatchInserter inserter = inserter( databaseDirectory, fileSystem, config, LoadKernelExtension() );
			  return new FileSystemClosingBatchInserter( inserter, ( IndexConfigStoreProvider ) inserter, fileSystem );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BatchInserter inserter(java.io.File databaseDirectory, org.neo4j.io.fs.FileSystemAbstraction fs, java.util.Map<String,String> config) throws java.io.IOException
		 public static BatchInserter Inserter( File databaseDirectory, FileSystemAbstraction fs, IDictionary<string, string> config )
		 {
			  return Inserter( databaseDirectory, fs, config, LoadKernelExtension() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BatchInserter inserter(java.io.File databaseDirectory, java.util.Map<String, String> config, Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions) throws java.io.IOException
		 public static BatchInserter Inserter<T1>( File databaseDirectory, IDictionary<string, string> config, IEnumerable<T1> kernelExtensions )
		 {
			  DefaultFileSystemAbstraction fileSystem = CreateFileSystem();
			  BatchInserterImpl inserter = new BatchInserterImpl( databaseDirectory, fileSystem, config, kernelExtensions );
			  return new FileSystemClosingBatchInserter( inserter, inserter, fileSystem );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static BatchInserter inserter(java.io.File databaseDirectory, org.neo4j.io.fs.FileSystemAbstraction fileSystem, java.util.Map<String,String> config, Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> kernelExtensions) throws java.io.IOException
		 public static BatchInserter Inserter<T1>( File databaseDirectory, FileSystemAbstraction fileSystem, IDictionary<string, string> config, IEnumerable<T1> kernelExtensions )
		 {
			  return new BatchInserterImpl( databaseDirectory, fileSystem, config, kernelExtensions );
		 }

		 private static DefaultFileSystemAbstraction CreateFileSystem()
		 {
			  return new DefaultFileSystemAbstraction();
		 }

		 private static System.Collections.IEnumerable LoadKernelExtension()
		 {
			  return Service.load( typeof( KernelExtensionFactory ) );
		 }
	}

}