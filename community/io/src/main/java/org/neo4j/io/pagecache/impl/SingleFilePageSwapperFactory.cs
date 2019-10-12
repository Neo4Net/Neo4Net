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

	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;

	/// <summary>
	/// A factory for SingleFilePageSwapper instances.
	/// </summary>
	/// <seealso cref= org.neo4j.io.pagecache.impl.SingleFilePageSwapper </seealso>
	public class SingleFilePageSwapperFactory : PageSwapperFactory
	{
		 private FileSystemAbstraction _fs;

		 public override void Open( FileSystemAbstraction fs, Configuration config )
		 {
			  this._fs = fs;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.pagecache.PageSwapper createPageSwapper(java.io.File file, int filePageSize, org.neo4j.io.pagecache.PageEvictionCallback onEviction, boolean createIfNotExist, boolean noChannelStriping) throws java.io.IOException
		 public override PageSwapper CreatePageSwapper( File file, int filePageSize, PageEvictionCallback onEviction, bool createIfNotExist, bool noChannelStriping )
		 {
			  if ( !_fs.fileExists( file ) )
			  {
					if ( createIfNotExist )
					{
						 _fs.create( file ).close();
					}
					else
					{
						 throw new NoSuchFileException( file.Path, null, "Cannot map non-existing file" );
					}
			  }
			  return new SingleFilePageSwapper( file, _fs, filePageSize, onEviction, noChannelStriping );
		 }

		 public override void SyncDevice()
		 {
			  // Nothing do to, since we `fsync` files individually in `force()`.
		 }

		 public override void Close()
		 {
			  // We have nothing to close
		 }

		 public override string ImplementationName()
		 {
			  return "single";
		 }

		 public virtual long RequiredBufferAlignment
		 {
			 get
			 {
				  return 1;
			 }
		 }
	}

}