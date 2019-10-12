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
namespace Org.Neo4j.@internal.Diagnostics
{
	using Logger = Org.Neo4j.Logging.Logger;

	public interface DiagnosticsProvider
	{
		 /// <summary>
		 /// Return an identifier for this <seealso cref="DiagnosticsProvider"/>. The result of
		 /// this method must be stable, i.e. invoking this method multiple times on
		 /// the same object should return <seealso cref="Object.equals(object) equal"/>
		 /// <seealso cref="string strings"/>.
		 /// 
		 /// For <seealso cref="DiagnosticsProvider"/>s where there is only one instance of that
		 /// <seealso cref="DiagnosticsProvider"/>, an implementation like this is would be a
		 /// sane default, given that the implementing class has a sensible name:
		 /// 
		 /// <code><pre>
		 /// public String getDiagnosticsIdentifier()
		 /// {
		 ///     return getClass().getName();
		 /// }
		 /// </pre></code>
		 /// </summary>
		 /// <returns> the identifier of this diagnostics provider. </returns>
		 string DiagnosticsIdentifier { get; }

		 /// <summary>
		 /// Accept a visitor that may or may not be capable of visiting this object.
		 /// 
		 /// Typical example:
		 /// 
		 /// <code><pre>
		 /// class OperationalStatistics implements <seealso cref="DiagnosticsProvider"/>
		 /// {
		 ///     public void <seealso cref="acceptDiagnosticsVisitor(object) acceptDiagnosticsVisitor"/>( <seealso cref="object"/> visitor )
		 ///     {
		 ///         if ( visitor instanceof OperationalStatisticsVisitor )
		 ///         {
		 ///              ((OperationalStatisticsVisitor)visitor).visitOperationalStatistics( this );
		 ///         }
		 ///     }
		 /// }
		 /// 
		 /// interface OperationalStatisticsVisitor
		 /// {
		 ///     void visitOperationalStatistics( OperationalStatistics statistics );
		 /// }
		 /// </pre></code>
		 /// </summary>
		 /// <param name="visitor"> the visitor visiting this <seealso cref="DiagnosticsProvider"/>. </param>
		 void AcceptDiagnosticsVisitor( object visitor );

		 /// <summary>
		 /// Dump the diagnostic information of this <seealso cref="DiagnosticsProvider"/> for
		 /// the specified <seealso cref="DiagnosticsPhase phase"/> to the provided
		 /// <seealso cref="Logger logger"/>.
		 /// </summary>
		 /// <param name="phase"> the <seealso cref="DiagnosticsPhase phase"/> to dump information for. </param>
		 /// <param name="logger"> the <seealso cref="Logger logger"/> to dump information to. </param>
		 void Dump( DiagnosticsPhase phase, Logger logger );
	}

}