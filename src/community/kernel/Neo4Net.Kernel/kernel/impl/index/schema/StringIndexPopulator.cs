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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using StoreIndexDescriptor = Neo4Net.Kernel.Api.StorageEngine.schema.StoreIndexDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.index.Internal.gbptree.GBPTree.NO_HEADER_WRITER;

	public class StringIndexPopulator : NativeIndexPopulator<StringIndexKey, NativeIndexValue>
	{
		 internal StringIndexPopulator( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<StringIndexKey, NativeIndexValue> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor ) : base( pageCache, fs, storeFile, layout, monitor, descriptor, NO_HEADER_WRITER )
		 {
		 }

		 internal override NativeIndexReader<StringIndexKey, NativeIndexValue> NewReader()
		 {
			  return new StringIndexReader( tree, layout, descriptor );
		 }
	}

}