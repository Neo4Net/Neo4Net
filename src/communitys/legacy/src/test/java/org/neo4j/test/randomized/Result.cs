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
namespace Neo4Net.Test.randomized
{
	public class Result<T, F>
	{
		 private readonly T _target;
		 private readonly int _index;
		 private readonly F _failure;

		 public Result( T target, int index, F failure )
		 {
			  this._target = target;
			  this._index = index;
			  this._failure = failure;
		 }

		 public virtual T Target
		 {
			 get
			 {
				  return _target;
			 }
		 }

		 public virtual int Index
		 {
			 get
			 {
				  return _index;
			 }
		 }

		 public virtual F Failure
		 {
			 get
			 {
				  return _failure;
			 }
		 }

		 public virtual bool Failure
		 {
			 get
			 {
				  return _index != -1;
			 }
		 }

		 public override string ToString()
		 {
			  return Failure ? "Failure[" + _index + ", " + _failure + "]" : "Success";
		 }
	}

}