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
namespace Neo4Net.Test.rule
{
	using RuleChain = org.junit.rules.RuleChain;
	using TestRule = org.junit.rules.TestRule;
	using Description = org.junit.runner.Description;
	using Statement = org.junit.runners.model.Statement;

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using Neo4Net.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	/// <summary>
	/// Very often when you want a <seealso cref="PageCacheRule"/> you also want <seealso cref="TestDirectory"/> and some <seealso cref="FileSystemRule"/>.
	/// This is tedious to write and apply in the correct order in every test doing this. This rule collects
	/// this threesome into one rule for convenience.
	/// </summary>
	public class PageCacheAndDependenciesRule : TestRule
	{
		 private RuleChain _chain;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private org.neo4j.test.rule.fs.FileSystemRule<? extends org.neo4j.io.fs.FileSystemAbstraction> fs;
		 private FileSystemRule<FileSystemAbstraction> _fs;
		 private TestDirectory _directory;
		 private PageCacheRule _pageCacheRule;
		 private PageCacheRule.PageCacheConfig _pageCacheConfig = config();
		 private Type _clazz;

		 public virtual PageCacheAndDependenciesRule With<T1>( FileSystemRule<T1> fs ) where T1 : Neo4Net.Io.fs.FileSystemAbstraction
		 {
			  this._fs = fs;
			  return this;
		 }

		 public virtual PageCacheAndDependenciesRule With( PageCacheRule.PageCacheConfig config )
		 {
			  this._pageCacheConfig = config;
			  return this;
		 }

		 public virtual PageCacheAndDependenciesRule With( Type clazz )
		 {
			  this._clazz = clazz;
			  return this;
		 }

		 public override Statement Apply( Statement @base, Description description )
		 {
			  if ( _chain == null )
			  {
					if ( _fs == null )
					{
						 _fs = new EphemeralFileSystemRule();
					}
					this._pageCacheRule = new PageCacheRule( _pageCacheConfig );
					this._directory = TestDirectory.TestDirectoryConflict( _clazz, _fs );
					this._chain = RuleChain.outerRule( _fs ).around( _directory ).around( _pageCacheRule );
			  }

			  return _chain.apply( @base, description );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: public org.neo4j.test.rule.fs.FileSystemRule<? extends org.neo4j.io.fs.FileSystemAbstraction> fileSystemRule()
		 public virtual FileSystemRule<FileSystemAbstraction> FileSystemRule()
		 {
			  return _fs;
		 }

		 public virtual FileSystemAbstraction FileSystem()
		 {
			  return _fs.get();
		 }

		 public virtual TestDirectory Directory()
		 {
			  return _directory;
		 }

		 public virtual PageCacheRule PageCacheRule()
		 {
			  return _pageCacheRule;
		 }

		 public virtual PageCache PageCache()
		 {
			  return _pageCacheRule.getPageCache( _fs );
		 }
	}

}