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
namespace Neo4Net.Kernel.Api.Index
{

	using IndexProviderDescriptor = Neo4Net.Kernel.Api.Internal.Schema.IndexProviderDescriptor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.io.fs.FileUtils.path;

	/// <summary>
	/// Dictates how directory structure looks for a <seealso cref="IndexProvider"/> and its indexes. Generally there's a
	/// <seealso cref="rootDirectory() root directory"/> which contains all index directories in some shape and form.
	/// For getting a directory (which must be a sub-directory to the root directory) for a particular index there's the
	/// <seealso cref="directoryForIndex(long)"/> method.
	/// 
	/// These instances are created from a <seealso cref="Factory"/> which typically gets passed into a <seealso cref="IndexProvider"/> constructor,
	/// which then creates a <seealso cref="IndexDirectoryStructure"/> given its <seealso cref="IndexProviderDescriptor"/>.
	/// </summary>
	public abstract class IndexDirectoryStructure
	{
		 /// <summary>
		 /// Creates an <seealso cref="IndexDirectoryStructure"/> for a <seealso cref="IndexProviderDescriptor"/> for a <seealso cref="IndexProvider"/>.
		 /// </summary>
		 public interface Factory
		 {
			  IndexDirectoryStructure ForProvider( IndexProviderDescriptor descriptor );
		 }

		 private class SubDirectoryByIndexId : IndexDirectoryStructure
		 {
			  internal readonly File ProviderRootFolder;

			  internal SubDirectoryByIndexId( File providerRootFolder )
			  {
					this.ProviderRootFolder = providerRootFolder;
			  }

			  public override File RootDirectory()
			  {
					return ProviderRootFolder;
			  }

			  public override File DirectoryForIndex( long indexId )
			  {
					return path( ProviderRootFolder, indexId.ToString() );
			  }
		 }

		 /// <summary>
		 /// Returns the base schema index directory, i.e.
		 /// 
		 /// <pre>
		 /// &lt;db&gt;/schema/index/
		 /// </pre>
		 /// </summary>
		 /// <param name="databaseStoreDir"> database store directory, i.e. {@code db} in the example above, where e.g. {@code nodestore} lives. </param>
		 /// <returns> the base directory of schema indexing. </returns>
		 public static File BaseSchemaIndexFolder( File databaseStoreDir )
		 {
			  return path( databaseStoreDir, "schema", "index" );
		 }

		 /// <param name="databaseStoreDir"> store directory of database, i.e. {@code db} in the example above. </param>
		 /// <returns> <seealso cref="Factory"/> for creating <seealso cref="IndexDirectoryStructure"/> returning directories looking something like:
		 /// 
		 /// <pre>
		 /// &lt;db&gt;/schema/index/&lt;providerKey&gt;/&lt;indexId&gt;/
		 /// </pre> </returns>
		 public static Factory DirectoriesByProviderKey( File databaseStoreDir )
		 {
			  return descriptor => new SubDirectoryByIndexId( path( BaseSchemaIndexFolder( databaseStoreDir ), fileNameFriendly( descriptor.Key ) ) );
		 }

		 /// <param name="databaseStoreDir"> store directory of database, i.e. {@code db} in the example above. </param>
		 /// <returns> <seealso cref="Factory"/> for creating <seealso cref="IndexDirectoryStructure"/> returning directories looking something like:
		 /// 
		 /// <pre>
		 /// &lt;db&gt;/schema/index/&lt;providerKey&gt;-&lt;providerVersion&gt;/&lt;indexId&gt;/
		 /// </pre> </returns>
		 public static Factory DirectoriesByProvider( File databaseStoreDir )
		 {
			  return descriptor => new SubDirectoryByIndexId( path( BaseSchemaIndexFolder( databaseStoreDir ), fileNameFriendly( descriptor ) ) );
		 }

		 /// <param name="directoryStructure"> existing <seealso cref="IndexDirectoryStructure"/>. </param>
		 /// <returns> a <seealso cref="Factory"/> returning an already existing <seealso cref="IndexDirectoryStructure"/>. </returns>
		 public static Factory Given( IndexDirectoryStructure directoryStructure )
		 {
			  return descriptor => directoryStructure;
		 }

		 /// <summary>
		 /// Useful when combining multiple <seealso cref="IndexProvider"/> into one.
		 /// </summary>
		 /// <param name="parentStructure"> <seealso cref="IndexDirectoryStructure"/> of the parent. </param>
		 /// <returns> <seealso cref="Factory"/> creating <seealso cref="IndexDirectoryStructure"/> looking something like:
		 /// 
		 /// <pre>
		 /// &lt;db&gt;/schema/index/.../&lt;indexId&gt;/&lt;childProviderKey&gt;-&lt;childProviderVersion&gt;/
		 /// </pre> </returns>
		 public static Factory DirectoriesBySubProvider( IndexDirectoryStructure parentStructure )
		 {
			  return descriptor => new IndexDirectoryStructureAnonymousInnerClass( parentStructure );
		 }

		 private class IndexDirectoryStructureAnonymousInnerClass : IndexDirectoryStructure
		 {
			 private Neo4Net.Kernel.Api.Index.IndexDirectoryStructure _parentStructure;

			 public IndexDirectoryStructureAnonymousInnerClass( Neo4Net.Kernel.Api.Index.IndexDirectoryStructure parentStructure )
			 {
				 this._parentStructure = parentStructure;
			 }

			 public override File rootDirectory()
			 {
				  return _parentStructure.rootDirectory();
			 }

			 public override File directoryForIndex( long indexId )
			 {
				  return path( _parentStructure.directoryForIndex( indexId ), fileNameFriendly( descriptor ) );
			 }
		 }

		 private static string FileNameFriendly( string name )
		 {
			  return name.replaceAll( "\\+", "_" );
		 }

		 private static string FileNameFriendly( IndexProviderDescriptor descriptor )
		 {
			  return FileNameFriendly( descriptor.Key + "-" + descriptor.Version );
		 }

		 private static readonly IndexDirectoryStructure NO_DIRECTORY_STRUCTURE = new IndexDirectoryStructureAnonymousInnerClass2();

		 private class IndexDirectoryStructureAnonymousInnerClass2 : IndexDirectoryStructure
		 {
			 public override File rootDirectory()
			 {
				  return null; // meaning there's no persistent storage
			 }

			 public override File directoryForIndex( long indexId )
			 {
				  return null; // meaning there's no persistent storage
			 }
		 }

		 /// <summary>
		 /// Useful for some in-memory index providers or similar.
		 /// </summary>
		 public static readonly Factory None = descriptor => NO_DIRECTORY_STRUCTURE;

		 /// <summary>
		 /// Returns root directory. Must be parent (one or more steps) to all sub-directories returned from <seealso cref="directoryForIndex(long)"/>.
		 /// Returns something equivalent to:
		 /// 
		 /// <pre>
		 /// &lt;db&gt;/schema/index/&lt;provider&gt;/
		 /// </pre>
		 /// </summary>
		 /// <returns> <seealso cref="File"/> denoting root directory for this provider.
		 /// May return {@code null} if there's no root directory, i.e. no persistent storage at all. </returns>
		 public abstract File RootDirectory();

		 /// <summary>
		 /// Returns a sub-directory (somewhere under <seealso cref="rootDirectory()"/>) for a specific index id, looking something equivalent to:
		 /// 
		 /// <pre>
		 /// &lt;db&gt;/schema/index/&lt;provider&gt;/&lt;indexId&gt;/
		 /// </pre>
		 /// 
		 /// I.e. the root of the schema indexes for this specific provider.
		 /// </summary>
		 /// <param name="indexId"> index id to return directory for. </param>
		 /// <returns> <seealso cref="File"/> denoting directory for the specific {@code indexId} for this provider.
		 /// May return {@code null} if there's no root directory, i.e. no persistent storage at all. </returns>
		 public abstract File DirectoryForIndex( long indexId );
	}

}