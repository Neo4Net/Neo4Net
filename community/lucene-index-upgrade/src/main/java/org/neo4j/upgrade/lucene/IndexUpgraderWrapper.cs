using System;
using System.Threading;

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
namespace Org.Neo4j.Upgrade.Lucene
{

	using EmbeddedJarLoader = Org.Neo4j.Upgrade.Loader.EmbeddedJarLoader;

	/// <summary>
	/// Index upgrader is a container for loaded lucene native upgrader.
	/// <para>
	/// During first attempt of index upgrade original migrator will be loaded and will be reused during further
	/// invocations.
	/// As soon as upgrade completed, index upgrader should be closed.
	/// 
	/// </para>
	/// </summary>
	/// <seealso cref= EmbeddedJarLoader </seealso>
	internal class IndexUpgraderWrapper : AutoCloseable
	{
		 private const string LUCENE_INDEX_UPGRADER_CLASS_NAME = "org.apache.lucene.index.IndexUpgrader";

		 private EmbeddedJarLoader _luceneLoader;
		 private MethodHandle _mainMethod;
		 private System.Func<EmbeddedJarLoader> _jarLoaderSupplier;

		 internal IndexUpgraderWrapper( System.Func<EmbeddedJarLoader> jarLoaderSupplier )
		 {
			  this._jarLoaderSupplier = jarLoaderSupplier;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void upgradeIndex(java.nio.file.Path indexPath) throws Throwable
		 public virtual void UpgradeIndex( Path indexPath )
		 {
			  // since lucene use ServiceLocator to load services, context class loader need to be replaced as well
			  ClassLoader contextClassLoader = Thread.CurrentThread.ContextClassLoader;
			  try
			  {
					if ( _mainMethod == null )
					{
						 _luceneLoader = _jarLoaderSupplier.get();
						 Type upgrader = _luceneLoader.loadEmbeddedClass( LUCENE_INDEX_UPGRADER_CLASS_NAME );
						 MethodHandles.Lookup lookup = MethodHandles.lookup();
						 _mainMethod = lookup.findStatic( upgrader, "main", MethodType.methodType( typeof( void ), typeof( string[] ) ) );
					}
					Thread.CurrentThread.ContextClassLoader = _luceneLoader.JarsClassLoader;
					_mainMethod.invokeExact( new string[]{ indexPath.ToString() } );
			  }
			  finally
			  {
					Thread.CurrentThread.ContextClassLoader = contextClassLoader;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  if ( _luceneLoader != null )
			  {
					_luceneLoader.close();
			  }
		 }
	}

}