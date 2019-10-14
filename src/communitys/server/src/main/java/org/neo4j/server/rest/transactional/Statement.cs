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
namespace Neo4Net.Server.rest.transactional
{

	public class Statement
	{
		 private readonly string _statement;
		 private readonly IDictionary<string, object> _parameters;
		 private readonly bool _includeStats;
		 private readonly ResultDataContent[] _resultDataContents;

		 public Statement( string statement, IDictionary<string, object> parameters, bool includeStats, params ResultDataContent[] resultDataContents )
		 {
			  this._statement = statement;
			  this._parameters = parameters;
			  this._includeStats = includeStats;
			  this._resultDataContents = resultDataContents;
		 }

//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
		 public virtual string StatementConflict()
		 {
			  return _statement;
		 }

		 public virtual IDictionary<string, object> Parameters()
		 {
			  return _parameters;
		 }

		 public virtual ResultDataContent[] ResultDataContents()
		 {
			  return _resultDataContents;
		 }

		 public virtual bool IncludeStats()
		 {
			  return _includeStats;
		 }
	}

}