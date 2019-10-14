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

	using Neo4Net.Index.@internal.gbptree;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexAccessor = Neo4Net.Kernel.Api.Index.IndexAccessor;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Neo4Net.Kernel.impl.util;
	using IndexReader = Neo4Net.Storageengine.Api.schema.IndexReader;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.GBPTree.NO_HEADER_WRITER;

	/// <summary>
	/// <seealso cref="IndexAccessor"/> using <seealso cref="StringLayout"/>, i.e for <seealso cref="string"/> values.
	/// </summary>
	public class StringIndexAccessor : NativeIndexAccessor<StringIndexKey, NativeIndexValue>
	{
		 private Validator<Value> _validator;

		 internal StringIndexAccessor( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<StringIndexKey, NativeIndexValue> layout, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, bool readOnly ) : base( pageCache, fs, storeFile, layout, monitor, descriptor, NO_HEADER_WRITER, readOnly )
		 {
			  instantiateTree( recoveryCleanupWorkCollector, HeaderWriter );
		 }

		 protected internal override void AfterTreeInstantiation( GBPTree<StringIndexKey, NativeIndexValue> tree )
		 {
			  _validator = new NativeIndexKeyLengthValidator<Value>( tree.KeyValueSizeCap(), layout );
		 }

		 public override IndexReader NewReader()
		 {
			  assertOpen();
			  return new StringIndexReader( tree, layout, descriptor );
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  _validator.validate( tuple[0] );
		 }
	}

}