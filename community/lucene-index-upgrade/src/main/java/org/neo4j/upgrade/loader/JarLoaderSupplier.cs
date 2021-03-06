﻿/*
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
namespace Org.Neo4j.Upgrade.Loader
{

	/// <summary>
	/// Supplier of embedded jar loaders. Will construct jar loader for specified set of jar paths. </summary>
	/// <seealso cref= EmbeddedJarLoader </seealso>
	public class JarLoaderSupplier : System.Func<EmbeddedJarLoader>
	{
		 private string[] _jarPaths;

		 public static JarLoaderSupplier Of( params string[] jarPaths )
		 {
			  return new JarLoaderSupplier( jarPaths );
		 }

		 private JarLoaderSupplier( params string[] jarPaths )
		 {
			  this._jarPaths = jarPaths;
		 }

		 public override EmbeddedJarLoader Get()
		 {
			  return new EmbeddedJarLoader( _jarPaths );
		 }
	}

}