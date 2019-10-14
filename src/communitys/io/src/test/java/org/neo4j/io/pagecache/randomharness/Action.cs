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
namespace Neo4Net.Io.pagecache.randomharness
{
	internal abstract class Action
	{
		 private readonly Command _command;
		 private readonly string _format;
		 private readonly object[] _parameters;
		 private readonly Action _innerAction;

		 protected internal Action( Command command, string format, params object[] parameters ) : this( command, null, format, parameters )
		 {
		 }

		 protected internal Action( Command command, Action innerAction, string format, params object[] parameters )
		 {
			  this._command = command;
			  this._format = format;
			  this._parameters = parameters;
			  this._innerAction = innerAction;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: abstract void perform() throws Exception;
		 internal abstract void Perform();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void performInnerAction() throws Exception
		 protected internal virtual void PerformInnerAction()
		 {
			  if ( _innerAction != null )
			  {
					_innerAction.perform();
			  }
		 }

		 public override string ToString()
		 {
			  if ( _innerAction == null )
			  {
					return string.format( _command + _format, _parameters );
			  }
			  else
			  {
					return string.format( _command + _format + ", and then " + _innerAction, _parameters );
			  }
		 }
	}

}