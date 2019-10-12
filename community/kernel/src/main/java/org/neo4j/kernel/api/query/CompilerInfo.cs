using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.api.query
{

	public class CompilerInfo
	{
		 private readonly string _planner;
		 private readonly string _runtime;
		 private readonly IList<IndexUsage> _indexes;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public CompilerInfo(@Nonnull String planner, @Nonnull String runtime, @Nonnull List<IndexUsage> indexes)
		 public CompilerInfo( string planner, string runtime, IList<IndexUsage> indexes )
		 {
			  this._planner = planner;
			  this._runtime = runtime;
			  this._indexes = indexes;
		 }

		 public virtual string Planner()
		 {
			  return _planner.ToLower();
		 }

		 public virtual string Runtime()
		 {
			  return _runtime.ToLower();
		 }

		 public virtual IList<IndexUsage> Indexes()
		 {
			  return _indexes;
		 }
	}

}