using System;

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
namespace Neo4Net.Kernel.@internal
{

	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;

	/// <summary>
	/// A <seealso cref="FileFilter"/> which only <seealso cref="accept(File) accepts"/> native index files.
	/// This class contains logic that is really index provider specific, but to ask index providers becomes tricky since
	/// they aren't always available and this filter is also expected to be used in offline scenarios.
	/// 
	/// The basic idea is to include everything except known lucene files (or directories known to include lucene files).
	/// </summary>
	public class NativeIndexFileFilter : FileFilter
	{
		 private readonly Path _indexRoot;

		 public NativeIndexFileFilter( File storeDir )
		 {
			  _indexRoot = IndexDirectoryStructure.baseSchemaIndexFolder( storeDir ).toPath().toAbsolutePath();
		 }

		 public override bool Accept( File file )
		 {
			  Path path = file.toPath();
			  if ( !path.toAbsolutePath().StartsWith(_indexRoot) )
			  {
					// This file isn't even under the schema/index root directory
					return false;
			  }

			  Path schemaPath = _indexRoot.relativize( path );
			  int nameCount = schemaPath.NameCount;

			  // - schema/index/lucene/.....
			  bool isPureLuceneProviderFile = nameCount >= 1 && schemaPath.getName( 0 ).ToString().Equals("lucene");
			  // - schema/index/lucene_native-x.y/<indexId>/lucene-x.y/x/.....
			  bool isFusionLuceneProviderFile = nameCount >= 3 && schemaPath.getName( 2 ).ToString().StartsWith("lucene-", StringComparison.Ordinal);

			  return !isPureLuceneProviderFile && !isFusionLuceneProviderFile;
		 }
	}

}