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
namespace Org.Neo4j.Kernel.impl.pagecache
{

	using Configuration = Org.Neo4j.Graphdb.config.Configuration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageSwapperFactory = Org.Neo4j.Io.pagecache.PageSwapperFactory;
	using SingleFilePageSwapperFactory = Org.Neo4j.Io.pagecache.impl.SingleFilePageSwapperFactory;

	public class PageSwapperFactoryForTesting : SingleFilePageSwapperFactory, PageSwapperFactory
	{
		 public const string TEST_PAGESWAPPER_NAME = "pageSwapperForTesting";

		 public static readonly AtomicInteger CreatedCounter = new AtomicInteger();
		 public static readonly AtomicInteger ConfiguredCounter = new AtomicInteger();

		 public static int CountCreatedPageSwapperFactories()
		 {
			  return CreatedCounter.get();
		 }

		 public static int CountConfiguredPageSwapperFactories()
		 {
			  return ConfiguredCounter.get();
		 }

		 public PageSwapperFactoryForTesting()
		 {
			  CreatedCounter.AndIncrement;
		 }

		 public override string ImplementationName()
		 {
			  return TEST_PAGESWAPPER_NAME;
		 }

		 public override void Open( FileSystemAbstraction fs, Configuration configuration )
		 {
			  base.Open( fs, configuration );
			  ConfiguredCounter.AndIncrement;
		 }
	}

}