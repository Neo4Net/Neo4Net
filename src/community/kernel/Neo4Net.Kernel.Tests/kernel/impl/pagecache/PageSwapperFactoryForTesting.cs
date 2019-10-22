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
namespace Neo4Net.Kernel.impl.pagecache
{

	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageSwapperFactory = Neo4Net.Io.pagecache.PageSwapperFactory;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;

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