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
namespace Neo4Net.Kernel.Api.Impl.Index.storage
{
	using Directory = org.apache.lucene.store.Directory;
	using FSDirectory = org.apache.lucene.store.FSDirectory;
	using FilterDirectory = org.apache.lucene.store.FilterDirectory;
	using NIOFSDirectory = org.apache.lucene.store.NIOFSDirectory;
	using NRTCachingDirectory = org.apache.lucene.store.NRTCachingDirectory;
	using RAMDirectory = org.apache.lucene.store.RAMDirectory;


	using FeatureToggles = Neo4Net.Utils.FeatureToggles;

	public interface DirectoryFactory : AutoCloseable
	{
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static DirectoryFactory directoryFactory(boolean ephemeral)
	//	 {
	//		  return ephemeral ? new DirectoryFactory.InMemoryDirectoryFactory() : DirectoryFactory.PERSISTENT;
	//	 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: org.apache.lucene.store.Directory open(java.io.File dir) throws java.io.IOException;
		 Directory Open( File dir );

	//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
	//	 DirectoryFactory PERSISTENT = new DirectoryFactory()
	//	 {
	//		  private final int MAX_MERGE_SIZE_MB = FeatureToggles.getInteger(DirectoryFactory.class, "max_merge_size_mb", 5);
	//		  private final int MAX_CACHED_MB = FeatureToggles.getInteger(DirectoryFactory.class, "max_cached_mb", 50);
	//		  private final boolean USE_DEFAULT_DIRECTORY_FACTORY = FeatureToggles.flag(DirectoryFactory.class, "default_directory_factory", true);
	//
	//		  @@SuppressWarnings("ResultOfMethodCallIgnored") @@Override public Directory open(File dir) throws IOException
	//		  {
	//				dir.mkdirs();
	//				FSDirectory directory = USE_DEFAULT_DIRECTORY_FACTORY ? FSDirectory.open(dir.toPath()) : new NIOFSDirectory(dir.toPath());
	//				return new NRTCachingDirectory(directory, MAX_MERGE_SIZE_MB, MAX_CACHED_MB);
	//		  }
	//
	//		  @@Override public void close()
	//		  {
	//				// No resources to release. This method only exists as a hook for test implementations.
	//		  }
	//
	//	 };
	}

	 public sealed class DirectoryFactory_InMemoryDirectoryFactory : DirectoryFactory
	 {
		  internal readonly IDictionary<File, RAMDirectory> Directories = new Dictionary<File, RAMDirectory>();

		  public override Directory Open( File dir )
		  {
			  lock ( this )
			  {
					if ( !Directories.ContainsKey( dir ) )
					{
						 Directories[dir] = new RAMDirectory();
					}
					return new DirectoryFactory_UncloseableDirectory( Directories[dir] );
			  }
		  }

		  public override void Close()
		  {
			  lock ( this )
			  {
					foreach ( RAMDirectory ramDirectory in Directories.Values )
					{
						 ramDirectory.close();
					}
					Directories.Clear();
			  }
		  }
	 }

	 public sealed class DirectoryFactory_Single : DirectoryFactory
	 {
		  internal readonly Directory Directory;

		  public DirectoryFactory_Single( Directory directory )
		  {
				this.Directory = directory;
		  }

		  public override Directory Open( File dir )
		  {
				return Directory;
		  }

		  public override void Close()
		  {
		  }
	 }

	 public sealed class DirectoryFactory_UncloseableDirectory : FilterDirectory
	 {

		  public DirectoryFactory_UncloseableDirectory( Directory @delegate ) : base( @delegate )
		  {
		  }

		  public override void Close()
		  {
				// No-op
		  }
	 }

}