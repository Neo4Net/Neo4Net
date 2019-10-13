﻿/*
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
namespace Neo4Net.Tooling.procedure.procedures.invalid.bad_record_type
{
	public class BadRecord
	{

		 private const long DEFAULT_AGE = 42L;
		 private readonly string _label; // nonstatic fields should be public
		 private readonly long _age;

		 public BadRecord( string label, long age )
		 {
			  this._label = label;
			  this._age = age < 0 ? DEFAULT_AGE : age;
		 }
	}

}